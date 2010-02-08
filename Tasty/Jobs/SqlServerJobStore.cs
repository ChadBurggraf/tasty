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
    using System.Linq;
    using Tasty.Configuration;  

    /// <summary>
    /// Implements <see cref="IJobStore"/> for SQL Server.
    /// </summary>
    public class SqlServerJobStore : IJobStore
    {
        #region Construction

        /// <summary>
        /// Initializes a new instance of the SqlServerJobStore class.
        /// </summary>
        public SqlServerJobStore()
        {
            this.ConnectionString = TastySettings.GetConnectionStringFromMetadata(TastySettings.Section.Jobs.Store.Metadata);
        }

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets or sets the connection string to use when connecting to the database.
        /// </summary>
        public string ConnectionString { get; set; }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Gets a collection of jobs that have been marked as <see cref="JobStatus.Canceling"/>.
        /// Opens a new transaction, then calls the delegate to perform any work. The transaction
        /// is committed when the delegate returns.
        /// </summary>
        /// <param name="ids">A collection of currently running job IDs.</param>
        /// <param name="canceling">The function to call with the canceling job collection.</param>
        public void CancelingJobs(IEnumerable<int> ids, Action<IEnumerable<JobRecord>> canceling)
        {
            this.DelegatedSetSelect(JobStatus.Canceling, ids, canceling);
        }

        /// <summary>
        /// Creates a new job record.
        /// </summary>
        /// <param name="record">The record to create.</param>
        /// <returns>The created record.</returns>
        public JobRecord CreateJob(JobRecord record)
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
        public void DequeueingJobs(int runsAvailable, Action<IEnumerable<JobRecord>> dequeueing)
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

                        dequeueing(JobStore.CreateRecordCollection(results));
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
        /// Gets a collection of jobs that have a status of <see cref="JobStatus.Started"/>.
        /// Opens a new transaction, then calls the delegate to perform any work. The transaction
        /// is committed when the delegate returns.
        /// </summary>
        /// <param name="ids">A collection of currently running job IDs.</param>
        /// <param name="finishing">The function to call with the finishing job collection.</param>
        public void FinishingJobs(IEnumerable<int> ids, Action<IEnumerable<JobRecord>> finishing)
        {
            this.DelegatedSetSelect(JobStatus.Started, ids, finishing);
        }

        /// <summary>
        /// Gets a single job record with the given ID.
        /// </summary>
        /// <param name="id">The ID of the job record to get.</param>
        /// <returns>The job record with the given ID, or null if none was found.</returns>
        public JobRecord GetJob(int id)
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

                        return JobStore.CreateRecordCollection(results).FirstOrDefault();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the single most recently queued job for each unique schedule name in the system.
        /// </summary>
        /// <returns>A collection of queued scheduled jobs.</returns>
        public IEnumerable<JobRecord> GetLatestScheduledJobs()
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

                        return JobStore.CreateRecordCollection(results);
                    }
                }
            }
        }

        /// <summary>
        /// Gets a collection of jobs that have a status of <see cref="JobStatus.Started"/>
        /// and can be timed out. Opens a new transaction, then calls the delegate to perform any work.
        /// The transaction is committed when the delegate returns.
        /// </summary>
        /// <param name="ids">A collection of currently running job IDs.</param>
        /// <param name="timingOut">The function to call with the timing-out job collection.</param>
        public void TimingOutJobs(IEnumerable<int> ids, Action<IEnumerable<JobRecord>> timingOut)
        {
            this.DelegatedSetSelect(JobStatus.Started, ids, timingOut);
        }

        /// <summary>
        /// Updates a collection of jobs. Opens a new transaction, then calls the delegate to perform
        /// any work on each record. The transaction is committed when all of the records have been iterated through.
        /// </summary>
        /// <param name="records">The records to update.</param>
        /// <param name="updating">The function to call for each iteration, which should perform any updates necessary on the job record.</param>
        public void UpdateJobs(IEnumerable<JobRecord> records, Action<JobRecord> updating)
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
                        JobStore.ParameterizeRecord<SqlCommand, SqlParameter>(record, command).ExecuteNonQuery();
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

            return JobStore.ParameterizeRecord<SqlCommand, SqlParameter>(record, command);
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Common delegated set select implementation (i.e., Canceling or Finishing).
        /// </summary>
        /// <param name="status">The <see cref="JobStatus"/> to filter the result set on.</param>
        /// <param name="ids">The ID set to filter the result set on.</param>
        /// <param name="delegating">The delegate action to perform with the result set.</param>
        private void DelegatedSetSelect(JobStatus status, IEnumerable<int> ids, Action<IEnumerable<JobRecord>> delegating)
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

                            delegating(JobStore.CreateRecordCollection(results));
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

        /// <summary>
        /// Ensures that a connection string is configured.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        private void EnsureConnectionString()
        {
            if (String.IsNullOrEmpty(this.ConnectionString))
            {
                string message = null;
                string connectionStringName = null;
                var keyValueElement = TastySettings.Section.Jobs.Store.Metadata["ConnectionStringName"];

                if (keyValueElement != null)
                {
                    connectionStringName = keyValueElement.Value;
                }

                if (!String.IsNullOrEmpty(connectionStringName))
                {
                    message = String.Format(CultureInfo.InvariantCulture, "You've specified that the current job store should use the connection string named \"{0}\", but there is either no connection string configured with that name or it is empty.", connectionStringName);
                }
                else
                {
                    message = "Please configure the name of the connection string to use for the Tasty.Jobs.SqlServerJobStore under /configuration/tasty/jobs/store[type=\"Tasty.Jobs.SqlServerJobStore, Tasty\"]/metadata/add[key=\"ConnectionStringName\"].";
                }

                throw new InvalidOperationException(message);
            }
        }

        #endregion
    }
}