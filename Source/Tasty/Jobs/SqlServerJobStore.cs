//-----------------------------------------------------------------------
// <copyright file="SqlServerJobStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Tasty.Configuration;  

    /// <summary>
    /// Implements <see cref="IJobStore"/> for SQL Server.
    /// </summary>
    public class SqlServerJobStore : SqlJobStore
    {
        #region Construction

        /// <summary>
        /// Initializes a new instance of the SqlServerJobStore class.
        /// </summary>
        public SqlServerJobStore()
            : this(TastySettings.GetConnectionStringFromMetadata(TastySettings.Section.Jobs.Store.Metadata))
        {
        }

        /// <summary>
        /// Initializes a new instance of the SqlServerJobStore class.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the database.</param>
        public SqlServerJobStore(string connectionString)
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

            using (SqlConnection connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = InsertCommand(record, connection))
                {
                    SqlParameter id = new SqlParameter("@Id", SqlDbType.Int);
                    id.Direction = ParameterDirection.Output;
                    command.Parameters.Add(id);

                    command.ExecuteNonQuery();
                    record.Id = (int)id.Value;
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

            const string Sql = "SELECT TOP {0} * FROM [TastyJob] WHERE [Status]='Queued' ORDER BY [QueueDate]";

            using (SqlConnection connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted, "TastyDequeueJobs");

                try
                {
                    SqlCommand command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;
                    command.CommandText = String.Format(CultureInfo.InvariantCulture, Sql, runsAvailable);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
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

            const string Sql = "SELECT TOP 1 * FROM [TastyJob] WHERE [Id] = @Id";

            using (SqlConnection connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = Sql;
                    command.Parameters.Add(new SqlParameter("@Id", id));

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
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
	                SELECT *, RANK() OVER (PARTITION BY [Type],[ScheduleName] ORDER BY [QueueDate] DESC) AS [Rank]
	                FROM [TastyJob]
	                WHERE
		                [ScheduleName] IS NOT NULL
                ) t WHERE [Rank] = 1";

            using (SqlConnection connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = Sql;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
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

            const string Sql = "UPDATE [TastyJob] SET [Name]=@Name,[Type]=@Type,[Data]=@Data,[Status]=@Status,[Exception]=@Exception,[QueueDate]=@QueueDate,[StartDate]=@StartDate,[FinishDate]=@FinishDate,[ScheduleName]=@ScheduleName WHERE [Id]=@Id";
            
            using (SqlConnection connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted, "TastyUpdateJobs");

                try
                {
                    foreach (var record in records)
                    {
                        if (updating != null)
                        {
                            updating(record);
                        }

                        SqlCommand command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandType = CommandType.Text;
                        command.CommandText = Sql;

                        command.Parameters.Add(new SqlParameter("@Id", record.Id.Value));
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
                    Id = (int)row["Id"],
                    Name = (string)row["Name"],
                    Data = (string)row["Data"],
                    Status = (JobStatus)Enum.Parse(typeof(JobStatus), (string)row["Status"]),
                    Exception = (row["Exception"] != DBNull.Value) ? (string)row["Exception"] : null,
                    QueueDate = new DateTime(((DateTime)row["QueueDate"]).Ticks, DateTimeKind.Utc),
                    StartDate = (DateTime?)(row["StartDate"] != DBNull.Value ? (DateTime?)new DateTime(((DateTime)row["StartDate"]).Ticks, DateTimeKind.Utc) : null),
                    FinishDate = (DateTime?)(row["FinishDate"] != DBNull.Value ? (DateTime?)new DateTime(((DateTime)row["FinishDate"]).Ticks, DateTimeKind.Utc) : null),
                    ScheduleName = (row["ScheduleName"] != DBNull.Value) ? (string)row["ScheduleName"] : null
                };

                try
                {
                    record.JobType = Type.GetType((string)row["Type"], true);
                }
                catch (FileNotFoundException ex)
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

            const string Sql = "SELECT * FROM [TastyJob] WHERE [Status]='{0}' AND [Id] IN ( {1} ) ORDER BY [QueueDate]";

            string[] jobIds = (from id in ids
                               select id.ToString(CultureInfo.InvariantCulture)).ToArray();

            if (jobIds.Length > 0)
            {
                using (SqlConnection connection = new SqlConnection(this.ConnectionString))
                {
                    connection.Open();

                    string transactionName = String.Format(CultureInfo.InvariantCulture, "Tasty{0}Jobs", status);
                    SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted, transactionName);

                    try
                    {
                        SqlCommand command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandType = CommandType.Text;
                        command.CommandText = String.Format(CultureInfo.InvariantCulture, Sql, status, String.Join(", ", jobIds));

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
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
        /// Creates a new <see cref="SqlCommand"/> for inserting the given <see cref="JobRecord"/> into the database.
        /// </summary>
        /// <param name="record">The record to create the command for.</param>
        /// <param name="connection">The connection to use when creating the command.</param>
        /// <returns>A new INSERT <see cref="SqlCommand"/>.</returns>
        private static SqlCommand InsertCommand(JobRecord record, SqlConnection connection)
        {
            const string Sql = "INSERT INTO [TastyJob]([Name],[Type],[Data],[Status],[Exception],[QueueDate],[StartDate],[FinishDate],[ScheduleName]) VALUES(@Name,@Type,@Data,@Status,@Exception,@QueueDate,@StartDate,@FinishDate,@ScheduleName); SET @Id=SCOPE_IDENTITY()";

            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = Sql;

            return ParameterizeRecord(record, command);
        }

        /// <summary>
        /// Parameterizes the given <see cref="JobRecord"/> into the given <see cref="SqlCommand"/> object.
        /// </summary>
        /// <param name="record">The <see cref="JobRecord"/> to parameterize.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to add <see cref="SqlParameter"/>s to.</param>
        /// <returns>The parameterized <see cref="SqlCommand"/>.</returns>
        private static SqlCommand ParameterizeRecord(JobRecord record, SqlCommand command)
        {
            command.Parameters.Add(new SqlParameter("@Name", record.Name));
            command.Parameters.Add(new SqlParameter("@Type", record.JobTypeString));
            command.Parameters.Add(new SqlParameter("@Data", record.Data));
            command.Parameters.Add(new SqlParameter("@Status", record.Status.ToString()));

            var exception = new SqlParameter("@Exception", record.Exception);

            if (String.IsNullOrEmpty(record.Exception))
            {
                exception.Value = DBNull.Value;
            }

            command.Parameters.Add(exception);
            command.Parameters.Add(new SqlParameter("@QueueDate", record.QueueDate));

            var startDate = new SqlParameter("@StartDate", record.StartDate);

            if (record.StartDate == null)
            {
                startDate.Value = DBNull.Value;
            }

            command.Parameters.Add(startDate);

            var finishDate = new SqlParameter("@FinishDate", record.FinishDate);

            if (record.FinishDate == null)
            {
                finishDate.Value = DBNull.Value;
            }

            command.Parameters.Add(finishDate);

            var scheduleName = new SqlParameter("@ScheduleName", record.ScheduleName);

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