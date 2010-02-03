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
            this.ConnectionString = ConfiguredConnectionString();
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
        /// <param name="canceling">The function to call with the canceling job collection.</param>
        public void CancelingJobs(Action<IEnumerable<JobRecord>> canceling)
        {
            this.EnsureConnectionString();

            const string Sql = "SELECT * FROM [TastyJob] WHERE [Status]='Canceling' ORDER BY [QueueDate]";

            using (SqlConnection connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted, "TastyCancellingJobs");

                try
                {
                    SqlCommand command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;
                    command.CommandText = Sql;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable results = new DataTable() { Locale = CultureInfo.InvariantCulture };
                        adapter.Fill(results);

                        canceling(RecordCollection(results));
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
        /// <param name="dequeueing">The function to call with the dequeued job collection.</param>
        /// <param name="runsAvailable">The maximum number of job job runs currently available, as determined by
        /// the <see cref="Tasty.Configuration.JobsElement.MaximumConcurrency"/> - the number of currently running jobs.</param>
        public void DequeueJobs(Action<IEnumerable<JobRecord>> dequeueing, int runsAvailable)
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

                        dequeueing(RecordCollection(results));
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
                        Parameterize(record, command).ExecuteNonQuery();
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
        /// Gets the currently configured connection string value.
        /// </summary>
        /// <returns>The currently configured connection string value.</returns>
        private static string ConfiguredConnectionString()
        {
            KeyValueConfigurationElement keyValueElement = TastySettings.Section.Jobs.Store.Metadata["ConnectionStringName"];
            string name = keyValueElement != null ? keyValueElement.Value : "LocalSqlServer";
            string connectionString = null;

            if (!String.IsNullOrEmpty(name))
            {
                ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[name];

                if (connectionStringSettings != null)
                {
                    connectionString = connectionStringSettings.ConnectionString;
                }
                else
                {
                    var appSetting = ConfigurationManager.AppSettings[name];

                    if (!String.IsNullOrEmpty(appSetting))
                    {
                        connectionString = appSetting;
                    }
                }
            }

            return connectionString;
        }

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

            return Parameterize(record, command);
        }

        /// <summary>
        /// Parameterizes the given <see cref="JobRecord"/> into the given <see cref="SqlCommand"/>.
        /// Does not include the record's ID.
        /// </summary>
        /// <param name="record">The record to parameterize.</param>
        /// <param name="command">The command to parameterize the record into.</param>
        /// <returns>The command with the record's values added as parameters.</returns>
        private static SqlCommand Parameterize(JobRecord record, SqlCommand command)
        {
            command.Parameters.Add(new SqlParameter("@Name", record.Name));
            command.Parameters.Add(new SqlParameter("@Type", record.JobType.AssemblyQualifiedName));
            command.Parameters.Add(new SqlParameter("@Data", record.Data));
            command.Parameters.Add(new SqlParameter("@Status", record.Status.ToString()));

            SqlParameter exception = new SqlParameter("@Exception", record.Exception);

            if (String.IsNullOrEmpty(record.Exception))
            {
                exception.Value = DBNull.Value;
            }

            command.Parameters.Add(exception);
            command.Parameters.Add(new SqlParameter("@QueueDate", record.QueueDate));

            SqlParameter startDate = new SqlParameter("@StartDate", record.StartDate);

            if (record.StartDate == null)
            {
                startDate.Value = DBNull.Value;
            }

            command.Parameters.Add(startDate);

            SqlParameter finishDate = new SqlParameter("@FinishDate", record.FinishDate);

            if (record.FinishDate == null)
            {
                finishDate.Value = DBNull.Value;
            }

            command.Parameters.Add(finishDate);

            SqlParameter scheduleName = new SqlParameter("@ScheduleName", record.ScheduleName);

            if (String.IsNullOrEmpty(record.ScheduleName))
            {
                scheduleName.Value = DBNull.Value;
            }

            command.Parameters.Add(scheduleName);

            return command;
        }

        /// <summary>
        /// Gets the given result set as a collection of <see cref="JobRecord"/>s
        /// </summary>
        /// <param name="resultSet">The result set to convert into a collection of <see cref="JobRecord"/>s.</param>
        /// <returns>A collection of <see cref="JobRecord"/>s.</returns>
        private static IEnumerable<JobRecord> RecordCollection(DataTable resultSet)
        {
            return from DataRow row in resultSet.Rows
                   select new JobRecord()
                   {
                       Id = (int)row["Id"],
                       Name = (string)row["Name"],
                       JobType = Type.GetType((string)row["Type"]),
                       Data = (string)row["Data"],
                       Status = (JobStatus)Enum.Parse(typeof(JobStatus), (string)row["Status"]),
                       Exception = (row["Exception"] != DBNull.Value) ? (string)row["Exception"] : null,
                       QueueDate = (DateTime)row["QueueDate"],
                       StartDate = (DateTime?)(row["StartDate"] != DBNull.Value ? row["StartDate"] : null),
                       FinishDate = (DateTime?)(row["FinishDate"] != DBNull.Value ? row["FinishDate"] : null),
                       ScheduleName = (row["ScheduleName"] != DBNull.Value) ? (string)row["ScheduleName"] : null
                   };
        }

        #endregion

        #region Private Instance Methods

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
                    message = String.Format(CultureInfo.InvariantCulture, "You've specified that the job store should use the connection string named \"{0}\", but there is either no connection string configured with that name or it is empty.", connectionStringName);
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