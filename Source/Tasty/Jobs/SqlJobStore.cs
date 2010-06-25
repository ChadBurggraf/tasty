

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using Tasty.Configuration;

    /// <summary>
    /// Base <see cref="IJobStore"/> implementation for implementors that use a connection string to connect to the database.
    /// </summary>
    public abstract class SqlJobStore : IJobStore
    {
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

        /// <summary>
        /// Gets or sets the connection string to use when connecting to the database.
        /// </summary>
        public string ConnectionString { get; set; }

        public void CommitTransaction()
        {
            throw new NotImplementedException();
        }

        public void DeleteJob(int id)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public JobRecord GetJob(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<JobRecord> GetJobs(IEnumerable<int> ids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<JobRecord> GetJobs(JobStatus status, int count)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<JobRecord> GetLatestScheduledJobs()
        {
            throw new NotImplementedException();
        }

        public void Initialize(TastySettings configuration)
        {
            throw new NotImplementedException();
        }

        public void RollbackTransaction()
        {
            throw new NotImplementedException();
        }

        public void SaveJob(JobRecord record)
        {
            throw new NotImplementedException();
        }

        public void StartTransaction()
        {
            throw new NotImplementedException();
        }
    }
}
