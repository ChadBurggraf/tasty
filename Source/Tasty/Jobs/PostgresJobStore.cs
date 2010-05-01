//-----------------------------------------------------------------------
// <copyright file="PostgresJobStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Npgsql;
    using Tasty.Configuration;

    /// <summary>
    /// Implements <see cref="IJobStore"/> for Postgres.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class PostgresJobStore : SqlJobStore
    {
        #region Construction

        /// <summary>
        /// Initializes a new instance of the PostgresJobStore class.
        /// </summary>
        public PostgresJobStore()
            : this(TastySettings.GetConnectionStringFromMetadata(TastySettings.Section.Jobs.Store.Metadata))
        {
        }

        /// <summary>
        /// Initializes a new instance of the PostgresJobStore class.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the database.</param>
        public PostgresJobStore(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Creates a new job record.
        /// </summary>
        /// <param name="record">The record to create.</param>
        /// <returns>The created record.</returns>
        public override JobRecord CreateJob(JobRecord record)
        {
            this.EnsureConnectionString();

            using (NpgsqlConnection connection = new NpgsqlConnection(this.ConnectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = InsertCommand(record, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            record.Id = Convert.ToInt32(reader[0], CultureInfo.InvariantCulture);
                        }
                    }
                }
            }

            return record;
        }

        /// <summary>
        /// Gets a collection of queued jobs that can be dequeued right now.
        /// Opens a new transaction, then calls the delegate to perform any work. The transaction
        /// is committed when the delegate returns.
        /// </summary>
        /// <param name="runsAvailable">The maximum number of job job runs currently available, as determined by
        /// the <see cref="Tasty.Configuration.JobsElement.MaximumConcurrency"/> - the number of currently running jobs.</param>
        /// <param name="dequeueing">The function to call with the dequeued job collection.</param>
        public override void DequeueingJobs(int runsAvailable, Action<IEnumerable<JobRecord>> dequeueing)
        {
            if (runsAvailable < 1)
            {
                throw new ArgumentException("runsAvailable must be greater than 0.", "runsAvailable");
            }

            this.EnsureConnectionString();

            const string Sql = "SELECT * FROM \"tasty_job\" WHERE \"status\"='Queued' ORDER BY \"queue_date\" LIMIT {0}";

            using (NpgsqlConnection connection = new NpgsqlConnection(this.ConnectionString))
            {
                connection.Open();
                NpgsqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                try
                {
                    NpgsqlCommand command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;
                    command.CommandText = String.Format(CultureInfo.InvariantCulture, Sql, runsAvailable);

                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
                    {
                        DataTable results = new DataTable() { Locale = CultureInfo.InvariantCulture };
                        adapter.Fill(results);

                        dequeueing(this.CreateRecordCollection(results));
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets a single job record with the given ID.
        /// </summary>
        /// <param name="id">The ID of the job record to get.</param>
        /// <returns>The job record with the given ID, or null if none was found.</returns>
        public override JobRecord GetJob(int id)
        {
            if (id < 1)
            {
                throw new ArgumentException("id must be greater than 0.", "id");
            }

            this.EnsureConnectionString();

            const string Sql = "SELECT * FROM \"tasty_job\" WHERE \"id\" = :id LIMIT 1";

            using (NpgsqlConnection connection = new NpgsqlConnection(this.ConnectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = Sql;
                    command.Parameters.Add(new NpgsqlParameter(":id", id));

                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
                    {
                        DataTable results = new DataTable() { Locale = CultureInfo.InvariantCulture };
                        adapter.Fill(results);

                        return this.CreateRecordCollection(results).FirstOrDefault();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the single most recently queued job for each unique schedule name in the system.
        /// </summary>
        /// <returns>A collection of queued scheduled jobs.</returns>
        public override IEnumerable<JobRecord> GetLatestScheduledJobs()
        {
            this.EnsureConnectionString();

            const string Sql =
                @"SELECT * FROM (
	                SELECT *, rank() OVER (PARTITION BY ""type"", ""schedule_name"" ORDER BY ""queue_date"" DESC)
	                FROM ""tasty_job""
	                WHERE
		                ""schedule_name"" IS NOT NULL
                ) AS t WHERE ""rank"" = 1";

            using (NpgsqlConnection connection = new NpgsqlConnection(this.ConnectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = Sql;

                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
                    {
                        DataTable results = new DataTable() { Locale = CultureInfo.InvariantCulture };
                        adapter.Fill(results);

                        return this.CreateRecordCollection(results);
                    }
                }
            }
        }

        /// <summary>
        /// Updates a collection of jobs. Opens a new transaction, then calls the delegate to perform
        /// any work on each record. The transaction is committed when all of the records have been iterated through.
        /// </summary>
        /// <param name="records">The records to update.</param>
        /// <param name="updating">The function to call for each iteration, which should perform any updates necessary on the job record.</param>
        public override void UpdateJobs(IEnumerable<JobRecord> records, Action<JobRecord> updating)
        {
            this.EnsureConnectionString();

            const string Sql = "UPDATE \"tasty_job\" SET \"name\"=:name,\"type\"=:type,\"data\"=:data,\"status\"=:status,\"exception\"=:exception,\"queue_date\"=:queue_date,\"start_date\"=:start_date,\"finish_date\"=:finish_date,\"schedule_name\"=:schedule_name WHERE \"id\"=:id";

            using (NpgsqlConnection connection = new NpgsqlConnection(this.ConnectionString))
            {
                connection.Open();
                NpgsqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                try
                {
                    foreach (var record in records)
                    {
                        if (updating != null)
                        {
                            updating(record);
                        }

                        NpgsqlCommand command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandType = CommandType.Text;
                        command.CommandText = Sql;

                        command.Parameters.Add(new NpgsqlParameter(":id", record.Id.Value));
                        ParameterizeRecord(record, command).ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion

        #region Protected Instance Methods

        /// <summary>
        /// Gets the given result set as a collection of <see cref="JobRecord"/>s.
        /// Assumes the result set has the expected schema definition.
        /// </summary>
        /// <param name="resultSet">The result set to convert into a collection of <see cref="JobRecord"/>s.</param>
        /// <returns>A collection of <see cref="JobRecord"/>s.</returns>
        protected override IEnumerable<JobRecord> CreateRecordCollection(DataTable resultSet)
        {
            List<JobRecord> records = new List<JobRecord>();

            foreach (DataRow row in resultSet.Rows)
            {
                JobRecord record = new JobRecord()
                {
                    Id = (int)row["id"],
                    Name = (string)row["name"],
                    Data = (string)row["data"],
                    Status = (JobStatus)Enum.Parse(typeof(JobStatus), (string)row["status"]),
                    Exception = (row["exception"] != DBNull.Value) ? (string)row["exception"] : null,
                    QueueDate = new DateTime(((DateTime)row["queue_date"]).Ticks, DateTimeKind.Utc),
                    StartDate = (DateTime?)(row["start_date"] != DBNull.Value ? (DateTime?)new DateTime(((DateTime)row["start_date"]).Ticks, DateTimeKind.Utc) : null),
                    FinishDate = (DateTime?)(row["finish_date"] != DBNull.Value ? (DateTime?)new DateTime(((DateTime)row["finish_date"]).Ticks, DateTimeKind.Utc) : null),
                    ScheduleName = (row["schedule_name"] != DBNull.Value) ? (string)row["schedule_name"] : null
                };

                try
                {
                    record.JobType = Type.GetType((string)row["type"], true);
                }
                catch (FileNotFoundException ex)
                {
                    record.Status = JobStatus.Failed;
                    record.Exception = new ExceptionXElement(ex).ToString();
                }
                catch (FileLoadException ex)
                {
                    record.Status = JobStatus.Failed;
                    record.Exception = new ExceptionXElement(ex).ToString();
                }
                catch (TypeLoadException ex)
                {
                    record.Status = JobStatus.Failed;
                    record.Exception = new ExceptionXElement(ex).ToString();
                }

                records.Add(record);
            }

            return records;
        }

        /// <summary>
        /// Common delegated set select implementation (i.e., Canceling or Finishing).
        /// </summary>
        /// <param name="status">The <see cref="JobStatus"/> to filter the result set on.</param>
        /// <param name="ids">The ID set to filter the result set on.</param>
        /// <param name="delegating">The delegate action to perform with the result set.</param>
        protected override void DelegatedSetSelect(JobStatus status, IEnumerable<int> ids, Action<IEnumerable<JobRecord>> delegating)
        {
            this.EnsureConnectionString();

            const string Sql = "SELECT * FROM \"tasty_job\" WHERE \"status\"='{0}' AND \"id\" IN ( {1} ) ORDER BY \"queue_date\"";

            string[] jobIds = (from id in ids
                               select id.ToString(CultureInfo.InvariantCulture)).ToArray();

            if (jobIds.Length > 0)
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(this.ConnectionString))
                {
                    connection.Open();

                    NpgsqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                    try
                    {
                        NpgsqlCommand command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandType = CommandType.Text;
                        command.CommandText = String.Format(CultureInfo.InvariantCulture, Sql, status, String.Join(", ", jobIds));

                        using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
                        {
                            DataTable results = new DataTable() { Locale = CultureInfo.InvariantCulture };
                            adapter.Fill(results);

                            delegating(this.CreateRecordCollection(results));
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            else
            {
                delegating(new JobRecord[0]);
            }
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Creates a new <see cref="NpgsqlCommand"/> for inserting the given <see cref="JobRecord"/> into the database.
        /// </summary>
        /// <param name="record">The record to create the command for.</param>
        /// <param name="connection">The connection to use when creating the command.</param>
        /// <returns>A new INSERT <see cref="NpgsqlCommand"/>.</returns>
        private static NpgsqlCommand InsertCommand(JobRecord record, NpgsqlConnection connection)
        {
            const string Sql = "INSERT INTO \"tasty_job\"(\"name\",\"type\",\"data\",\"status\",\"exception\",\"queue_date\",\"start_date\",\"finish_date\",\"schedule_name\") VALUES(:name,:type,:data,:status,:exception,:queue_date,:start_date,:finish_date,:schedule_name); SELECT currval('tasty_job_id_seq') AS \"id\";";

            NpgsqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = Sql;

            return ParameterizeRecord(record, command);
        }

        /// <summary>
        /// Parameterizes the given <see cref="JobRecord"/> into the given <see cref="NpgsqlCommand"/> object.
        /// </summary>
        /// <param name="record">The <see cref="JobRecord"/> to parameterize.</param>
        /// <param name="command">The <see cref="NpgsqlCommand"/> to add <see cref="NpgsqlParameter"/>s to.</param>
        /// <returns>The parameterized <see cref="NpgsqlCommand"/>.</returns>
        private static NpgsqlCommand ParameterizeRecord(JobRecord record, NpgsqlCommand command)
        {
            command.Parameters.Add(new NpgsqlParameter(":name", record.Name));
            command.Parameters.Add(new NpgsqlParameter(":type", record.JobTypeString));
            command.Parameters.Add(new NpgsqlParameter(":data", record.Data));
            command.Parameters.Add(new NpgsqlParameter(":status", record.Status.ToString()));

            var exception = new NpgsqlParameter(":exception", record.Exception);

            if (String.IsNullOrEmpty(record.Exception))
            {
                exception.Value = DBNull.Value;
            }

            command.Parameters.Add(exception);
            command.Parameters.Add(new NpgsqlParameter(":queue_date", record.QueueDate));

            var startDate = new NpgsqlParameter(":start_date", record.StartDate);

            if (record.StartDate == null)
            {
                startDate.Value = DBNull.Value;
            }

            command.Parameters.Add(startDate);

            var finishDate = new NpgsqlParameter(":finish_date", record.FinishDate);

            if (record.FinishDate == null)
            {
                finishDate.Value = DBNull.Value;
            }

            command.Parameters.Add(finishDate);

            var scheduleName = new NpgsqlParameter(":schedule_name", record.ScheduleName);

            if (String.IsNullOrEmpty(record.ScheduleName))
            {
                scheduleName.Value = DBNull.Value;
            }

            command.Parameters.Add(scheduleName);

            return command;
        }

        #endregion
    }
}
