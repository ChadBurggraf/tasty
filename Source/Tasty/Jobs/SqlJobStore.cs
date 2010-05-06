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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Tasty.Configuration;

    /// <summary>
    /// Base class for <see cref="IJobStore"/> implementors that use a connection string to connect to a database.
    /// </summary>
    public abstract class SqlJobStore : IJobStore
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
        public virtual void CancelingJobs(IEnumerable<int> ids, Action<IEnumerable<JobRecord>> canceling)
        {
            this.DelegatedSetSelect(JobStatus.Canceling, ids, canceling);
        }

        /// <summary>
        /// Creates a new job record.
        /// </summary>
        /// <param name="record">The record to create.</param>
        /// <returns>The created record.</returns>
        public abstract JobRecord CreateJob(JobRecord record);

        /// <summary>
        /// Deletes a job record from the job store.
        /// </summary>
        /// <param name="id">The ID of the job to delete.</param>
        public abstract void DeleteJob(int id);

        /// <summary>
        /// Gets a collection of queued jobs that can be dequeued right now.
        /// Opens a new transaction, then calls the delegate to perform any work. The transaction
        /// is committed when the delegate returns.
        /// </summary>
        /// <param name="runsAvailable">The maximum number of job job runs currently available, as determined by
        /// the <see cref="Tasty.Configuration.JobsElement.MaximumConcurrency"/> - the number of currently running jobs.</param>
        /// <param name="dequeueing">The function to call with the dequeued job collection.</param>
        public abstract void DequeueingJobs(int runsAvailable, Action<IEnumerable<JobRecord>> dequeueing);

        /// <summary>
        /// Gets a collection of jobs that have a status of <see cref="JobStatus.Started"/>.
        /// Opens a new transaction, then calls the delegate to perform any work. The transaction
        /// is committed when the delegate returns.
        /// </summary>
        /// <param name="ids">A collection of currently running job IDs.</param>
        /// <param name="finishing">The function to call with the finishing job collection.</param>
        public virtual void FinishingJobs(IEnumerable<int> ids, Action<IEnumerable<JobRecord>> finishing)
        {
            this.DelegatedSetSelect(JobStatus.Started, ids, finishing);
        }

        /// <summary>
        /// Gets a single job record with the given ID.
        /// </summary>
        /// <param name="id">The ID of the job record to get.</param>
        /// <returns>The job record with the given ID, or null if none was found.</returns>
        public abstract JobRecord GetJob(int id);

        /// <summary>
        /// Gets the single most recently queued job for each unique schedule name in the system.
        /// </summary>
        /// <returns>A collection of queued scheduled jobs.</returns>
        public abstract IEnumerable<JobRecord> GetLatestScheduledJobs();

        /// <summary>
        /// Gets a collection of jobs that have a status of <see cref="JobStatus.Started"/>
        /// and can be timed out. Opens a new transaction, then calls the delegate to perform any work.
        /// The transaction is committed when the delegate returns.
        /// </summary>
        /// <param name="ids">A collection of currently running job IDs.</param>
        /// <param name="timingOut">The function to call with the timing-out job collection.</param>
        public virtual void TimingOutJobs(IEnumerable<int> ids, Action<IEnumerable<JobRecord>> timingOut)
        {
            this.DelegatedSetSelect(JobStatus.Started, ids, timingOut);
        }

        /// <summary>
        /// Updates a collection of jobs. Opens a new transaction, then calls the delegate to perform
        /// any work on each record. The transaction is committed when all of the records have been iterated through.
        /// </summary>
        /// <param name="records">The records to update.</param>
        /// <param name="updating">The function to call for each iteration, which should perform any updates necessary on the job record.</param>
        public abstract void UpdateJobs(IEnumerable<JobRecord> records, Action<JobRecord> updating);

        #endregion

        #region Protected Instance Methods

        /// <summary>
        /// Gets the given result set as a collection of <see cref="JobRecord"/>s.
        /// Assumes the result set has the expected schema definition.
        /// </summary>
        /// <param name="resultSet">The result set to convert into a collection of <see cref="JobRecord"/>s.</param>
        /// <returns>A collection of <see cref="JobRecord"/>s.</returns>
        protected abstract IEnumerable<JobRecord> CreateRecordCollection(DataTable resultSet);

        /// <summary>
        /// Common delegated set select implementation (i.e., Canceling or Finishing).
        /// </summary>
        /// <param name="status">The <see cref="JobStatus"/> to filter the result set on.</param>
        /// <param name="ids">The ID set to filter the result set on.</param>
        /// <param name="delegating">The delegate action to perform with the result set.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "I'm not strongly typing a delegate when this will work just fine.")]
        protected abstract void DelegatedSetSelect(JobStatus status, IEnumerable<int> ids, Action<IEnumerable<JobRecord>> delegating);

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
                        "Please configure the name of the connection string to use for the {0} under /configuration/tasty/jobs/store[type=\"Tasty.Jobs.SqlServerJobStore, Tasty\"]/metadata/add[key=\"ConnectionStringName\"].",
                        GetType().FullName);
                }

                throw new InvalidOperationException(message);
            }
        }

        #endregion
    }
}
