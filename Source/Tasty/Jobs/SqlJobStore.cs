//-----------------------------------------------------------------------
// <copyright file="SqlJobStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.Linq;
    using Tasty.Configuration;

    /// <summary>
    /// Implements <see cref="IJobStore"/> as the base class for SQL job stores.
    /// </summary>
    public abstract class SqlJobStore : JobStore
    {
        #region Construction

        /// <summary>
        /// Initializes a new instance of the SqlJobStore class.
        /// </summary>
        protected SqlJobStore()
            : this(TastySettings.GetConnectionStringFromMetadata(TastySettings.Section.Jobs.Store.Metadata))
        {
        }

        /// <summary>
        /// Initializes a new instance of the SqlJobStore class.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the database.</param>
        protected SqlJobStore(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets the connection string to use when connecting to the database.
        /// </summary>
        public string ConnectionString { get; private set; }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Creates a data adapter.
        /// </summary>
        /// <param name="command">The command to create the adapter with.</param>
        /// <returns>The created data adapter.</returns>
        public abstract DataAdapter CreateAdapter(DbCommand command);

        /// <summary>
        /// Creates a connection.
        /// </summary>
        /// <returns>The created connection.</returns>
        public abstract DbConnection CreateConnection();

        /// <summary>
        /// Creates a delete command.
        /// </summary>
        /// <param name="connection">The connection to create the command with.</param>
        /// <param name="transaction">The transaction to create the command with, if applicable.</param>
        /// <param name="id">The ID of the record to delete.</param>
        /// <returns>A delete command.</returns>
        public abstract DbCommand CreateDeleteCommand(DbConnection connection, DbTransaction transaction, int id);

        /// <summary>
        /// Creates a select command that can be used to fetch a collection of each scheduled job's latest record.
        /// </summary>
        /// <param name="scheduleNames">A collection of schedule names to get the latest persisted jobs for.</param>
        /// <param name="connection">The connection to create the command with.</param>
        /// <returns>A select command.</returns>
        public abstract DbCommand CreateLatestScheduledJobsSelectCommand(IEnumerable<string> scheduleNames, DbConnection connection);

        /// <summary>
        /// Creates an insert or update command.
        /// </summary>
        /// <param name="connection">The connection to create the command with.</param>
        /// <param name="transaction">The transaction to create the command with, if applicable.</param>
        /// <param name="record">The record to insert or update.</param>
        /// <returns>An insert or update command.</returns>
        public abstract DbCommand CreateSaveCommand(DbConnection connection, DbTransaction transaction, JobRecord record);

        /// <summary>
        /// Creates a collection of <see cref="JobRecord"/>s from the given <see cref="DataSet"/>.
        /// </summary>
        /// <param name="resultSet">The <see cref="DataSet"/> to create the records from.</param>
        /// <returns>A collection of <see cref="JobRecord"/>s</returns>
        public abstract IEnumerable<JobRecord> CreateRecordCollection(DataSet resultSet);

        /// <summary>
        /// Creates a select command.
        /// </summary>
        /// <param name="connection">The connection to create the command with.</param>
        /// <param name="id">The ID of the result to fetch.</param>
        /// <returns>A select command.</returns>
        public abstract DbCommand CreateSelectCommand(DbConnection connection, int id);

        /// <summary>
        /// Creates a select command.
        /// </summary>
        /// <param name="connection">The connection to create the command with.</param>
        /// <param name="ids">The IDs to restrict the result set to.</param>
        /// <returns>A select command.</returns>
        public abstract DbCommand CreateSelectCommand(DbConnection connection, IEnumerable<int> ids);

        /// <summary>
        /// Creates a select command.
        /// </summary>
        /// <param name="connection">The connection to create the command with.</param>
        /// <param name="status">The job status to filter results on.</param>
        /// <param name="count">The maximum number of results to select.</param>
        /// <returns>A select command.</returns>
        public abstract DbCommand CreateSelectCommand(DbConnection connection, JobStatus status, int count);

        /// <summary>
        /// Deletes a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to delete.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        public override void DeleteJob(int id, IJobStoreTransaction transaction)
        {
            if (transaction != null)
            {
                transaction.AddForDelete(id);
            }
            else
            {
                using (DbConnection connection = this.CreateConnection())
                {
                    connection.Open();

                    using (DbCommand command = this.CreateDeleteCommand(connection, null, id))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Gets a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to get.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        /// <returns>The job with the given ID.</returns>
        public override JobRecord GetJob(int id, IJobStoreTransaction transaction)
        {
            if (transaction != null)
            {
                SqlJobStoreTransaction concreteTransaction = (SqlJobStoreTransaction)transaction;
                return this.GetJob(id, concreteTransaction.Connection, concreteTransaction.Transaction);
            }
            else
            {
                using (DbConnection connection = this.CreateConnection())
                {
                    connection.Open();
                    return this.GetJob(id, connection, null);
                }
            }
        }

        /// <summary>
        /// Gets a collection of jobs that match the given collection of IDs.
        /// </summary>
        /// <param name="ids">The IDs of the jobs to get.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        /// <returns>A collection of jobs.</returns>
        public override IEnumerable<JobRecord> GetJobs(IEnumerable<int> ids, IJobStoreTransaction transaction)
        {
            if (ids != null && ids.Count() > 0)
            {
                if (transaction != null)
                {
                    SqlJobStoreTransaction concreteTransaction = (SqlJobStoreTransaction)transaction;
                    return this.GetJobs(ids, concreteTransaction.Connection, concreteTransaction.Transaction);
                }
                else
                {
                    using (DbConnection connection = this.CreateConnection())
                    {
                        connection.Open();
                        return this.GetJobs(ids, connection, null);
                    }
                }
            }

            return new JobRecord[0];
        }

        /// <summary>
        /// Gets a collection of jobs with the given status, returning
        /// at most the number of jobs identified by <paramref name="count"/>.
        /// </summary>
        /// <param name="status">The status of the jobs to get.</param>
        /// <param name="count">The maximum number of jobs to get.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        /// <returns>A collection of jobs.</returns>
        public override IEnumerable<JobRecord> GetJobs(JobStatus status, int count, IJobStoreTransaction transaction)
        {
            if (transaction != null)
            {
                SqlJobStoreTransaction concreteTransaction = (SqlJobStoreTransaction)transaction;
                return this.GetJobs(status, count, concreteTransaction.Connection, concreteTransaction.Transaction);
            }
            else
            {
                using (DbConnection connection = this.CreateConnection())
                {
                    connection.Open();
                    return this.GetJobs(status, count, connection, null);
                }
            }
        }

        /// <summary>
        /// Gets a collection of the most recently scheduled persisted job for each
        /// scheduled job in the given collection.
        /// </summary>
        /// <param name="scheduleNames">A collection of schedule names to get the latest persisted jobs for.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        /// <returns>A collection of recently scheduled jobs.</returns>
        public override IEnumerable<JobRecord> GetLatestScheduledJobs(IEnumerable<string> scheduleNames, IJobStoreTransaction transaction)
        {
            string[] names = scheduleNames != null ? scheduleNames.ToArray() : new string[0];

            if (names.Length > 0)
            {
                if (transaction != null)
                {
                    SqlJobStoreTransaction concreteTransaction = (SqlJobStoreTransaction)transaction;
                    return this.GetLatestScheduledJobs(names, concreteTransaction.Connection, concreteTransaction.Transaction);
                }
                else
                {
                    using (DbConnection connection = this.CreateConnection())
                    {
                        connection.Open();
                        return this.GetLatestScheduledJobs(names, connection, null);
                    }
                }
            }

            return new JobRecord[0];
        }

        /// <summary>
        /// Saves the given job record, either creating it or updating it.
        /// </summary>
        /// <param name="record">The job to save.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        public override void SaveJob(JobRecord record, IJobStoreTransaction transaction)
        {
            if (transaction != null)
            {
                transaction.AddForSave(record);
            }
            else
            {
                using (DbConnection connection = this.CreateConnection())
                {
                    connection.Open();

                    using (DbCommand command = this.CreateSaveCommand(connection, null, record))
                    {
                        using (DbDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                record.Id = Convert.ToInt32(reader[0], CultureInfo.InvariantCulture);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Starts a transaction.
        /// </summary>
        public override IJobStoreTransaction StartTransaction()
        {
            DbConnection connection = this.CreateConnection();
            connection.Open();

            return new SqlJobStoreTransaction(this, connection, connection.BeginTransaction());
        }

        #endregion

        #region Protected Instance Methods

        /// <summary>
        /// Ensures that a connection string is configured.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        protected void EnsureConnectionString()
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
                    message = String.Format(
                        CultureInfo.InvariantCulture,
                        "Please configure the name of the connection string to use for the {0} under /configuration/tasty/jobs/store[type=\"{0}, {1}\"]/metadata/add[key=\"ConnectionStringName\"].",
                        GetType().FullName,
                        GetType().Assembly.GetName().Name);
                }

                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Gets a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to get.</param>
        /// <param name="connection">The concrete connection to use.</param>
        /// <param name="transaction">The concrete transaction to use.</param>
        /// <returns>The job with the given ID.</returns>
        protected virtual JobRecord GetJob(int id, DbConnection connection, DbTransaction transaction)
        {
            using (DbCommand command = this.CreateSelectCommand(connection, id))
            {
                command.Transaction = transaction;

                using (DataAdapter adapter = this.CreateAdapter(command))
                {
                    DataSet results = new DataSet() { Locale = CultureInfo.InvariantCulture };
                    adapter.Fill(results);

                    return this.CreateRecordCollection(results).FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// Gets a collection of jobs that match the given collection of IDs.
        /// </summary>
        /// <param name="ids">The IDs of the jobs to get.</param>
        /// <param name="connection">The concrete connection to use.</param>
        /// <param name="transaction">The concrete transaction to use.</param>
        /// <returns>A collection of jobs.</returns>
        protected virtual IEnumerable<JobRecord> GetJobs(IEnumerable<int> ids, DbConnection connection, DbTransaction transaction)
        {
            using (DbCommand command = this.CreateSelectCommand(connection, ids))
            {
                command.Transaction = transaction;

                using (DataAdapter adapter = this.CreateAdapter(command))
                {
                    DataSet results = new DataSet() { Locale = CultureInfo.InvariantCulture };
                    adapter.Fill(results);

                    return this.CreateRecordCollection(results);
                }
            }
        }

        /// <summary>
        /// Gets a collection of jobs with the given status, returning
        /// at most the number of jobs identified by <paramref name="count"/>.
        /// </summary>
        /// <param name="status">The status of the jobs to get.</param>
        /// <param name="count">The maximum number of jobs to get.</param>
        /// <param name="connection">The concrete connection to use.</param>
        /// <param name="transaction">The concrete transaction to use.</param>
        /// <returns>A collection of jobs.</returns>
        protected virtual IEnumerable<JobRecord> GetJobs(JobStatus status, int count, DbConnection connection, DbTransaction transaction)
        {
            using (DbCommand command = this.CreateSelectCommand(connection, status, count))
            {
                command.Transaction = transaction;

                using (DataAdapter adapter = this.CreateAdapter(command))
                {
                    DataSet results = new DataSet() { Locale = CultureInfo.InvariantCulture };
                    adapter.Fill(results);

                    return this.CreateRecordCollection(results);
                }
            }
        }

        /// <summary>
        /// Gets a collection of the most recently scheduled persisted job for each
        /// scheduled job in the configuration.
        /// </summary>
        /// <param name="scheduleNames">A collection of schedule names to get the latest persisted jobs for.</param>
        /// <param name="connection">The concrete connection to use.</param>
        /// <param name="transaction">The concrete transaction to use.</param>
        /// <returns>A collection of recently scheduled jobs.</returns>
        protected virtual IEnumerable<JobRecord> GetLatestScheduledJobs(IEnumerable<string> scheduleNames, DbConnection connection, DbTransaction transaction)
        {
            using (DbCommand command = this.CreateLatestScheduledJobsSelectCommand(scheduleNames, connection))
            {
                command.Transaction = transaction;

                using (DataAdapter adapter = this.CreateAdapter(command))
                {
                    DataSet results = new DataSet() { Locale = CultureInfo.InvariantCulture };
                    adapter.Fill(results);

                    return this.CreateRecordCollection(results);
                }
            }
        }

        #endregion
    }
}
