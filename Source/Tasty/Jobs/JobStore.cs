//-----------------------------------------------------------------------
// <copyright file="JobStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Tasty.Configuration;

    /// <summary>
    /// Provides a base <see cref="IJobStore"/> implementation.
    /// </summary>
    public abstract class JobStore : IJobStore
    {
        private static readonly object locker = new object();
        private static IJobStore current;
        private string typeKey;

        /// <summary>
        /// Gets or sets the current <see cref="IJobStore"/> implementation in use.
        /// The setter on this property is primarily meant for testing purposes.
        /// </summary>
        /// <remarks>
        /// It is not recommended to set this property during runtime. You should instead
        /// set it during static initialization if you would rather not infer it from
        /// the configuration. Setting it later could cause persistence errors if any
        /// currently-executing jobs try to persist their update data to the new store.
        /// </remarks>
        public static IJobStore Current
        {
            get
            {
                lock (locker)
                {
                    if (current == null)
                    {
                        current = (IJobStore)Activator.CreateInstance(Type.GetType(TastySettings.Section.Jobs.Store.JobStoreType));
                    }

                    return current;
                }
            }

            set
            {
                lock (locker)
                {
                    current = value;
                }
            }
        }
            /// <summary>
        /// Gets a unique identifier for this <see cref="IJobStore"/> implementation that
        /// can be used to isolation job runners and running jobs peristence providers.
        /// </summary>
        public string TypeKey 
        {
            get
            {
                lock (this)
                {
                    if (this.typeKey == null)
                    {
                        Type type = GetType();
                        this.typeKey = String.Concat(type.FullName, ", ", type.Assembly.GetName().Name);
                    }

                    return this.typeKey;
                }
            }
        }

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Deletes a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to delete.</param>
        public virtual void DeleteJob(int id)
        {
            this.DeleteJob(id, null);
        }

        /// <summary>
        /// Deletes a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to delete.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        public abstract void DeleteJob(int id, IJobStoreTransaction transaction);

        /// <summary>
        /// Gets a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to get.</param>
        /// <returns>The job with the given ID.</returns>
        public virtual JobRecord GetJob(int id)
        {
            return this.GetJob(id, null);
        }

        /// <summary>
        /// Gets a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to get.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        /// <returns>The job with the given ID.</returns>
        public abstract JobRecord GetJob(int id, IJobStoreTransaction transaction);

        /// <summary>
        /// Gets a collection of jobs that match the given collection of IDs.
        /// </summary>
        /// <param name="ids">The IDs of the jobs to get.</param>
        /// <returns>A collection of jobs.</returns>
        public virtual IEnumerable<JobRecord> GetJobs(IEnumerable<int> ids)
        {
            return this.GetJobs(ids, null);
        }

        /// <summary>
        /// Gets a collection of jobs that match the given collection of IDs.
        /// </summary>
        /// <param name="ids">The IDs of the jobs to get.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        /// <returns>A collection of jobs.</returns>
        public abstract IEnumerable<JobRecord> GetJobs(IEnumerable<int> ids, IJobStoreTransaction transaction);

        /// <summary>
        /// Gets a collection of jobs with the given status, returning
        /// at most the number of jobs identified by <paramref name="count"/>.
        /// </summary>
        /// <param name="status">The status of the jobs to get.</param>
        /// <param name="count">The maximum number of jobs to get.</param>
        /// <returns>A collection of jobs.</returns>
        public virtual IEnumerable<JobRecord> GetJobs(JobStatus status, int count)
        {
            return this.GetJobs(status, count, null);
        }

        /// <summary>
        /// Gets a collection of jobs with the given status, returning
        /// at most the number of jobs identified by <paramref name="count"/>.
        /// </summary>
        /// <param name="status">The status of the jobs to get.</param>
        /// <param name="count">The maximum number of jobs to get.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        /// <returns>A collection of jobs.</returns>
        public abstract IEnumerable<JobRecord> GetJobs(JobStatus status, int count, IJobStoreTransaction transaction);

        /// <summary>
        /// Gets a collection of the most recently scheduled persisted job for each
        /// scheduled job in the given collection.
        /// </summary>
        /// <param name="scheduleNames">A collection of schedule names to get the latest persisted jobs for.</param>
        /// <returns>A collection of recently scheduled jobs.</returns>
        public virtual IEnumerable<JobRecord> GetLatestScheduledJobs(IEnumerable<string> scheduleNames)
        {
            return this.GetLatestScheduledJobs(scheduleNames, null);
        }

        /// <summary>
        /// Gets a collection of the most recently scheduled persisted job for each
        /// scheduled job in the given collection.
        /// </summary>
        /// <param name="scheduleNames">A collection of schedule names to get the latest persisted jobs for.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        /// <returns>A collection of recently scheduled jobs.</returns>
        public abstract IEnumerable<JobRecord> GetLatestScheduledJobs(IEnumerable<string> scheduleNames, IJobStoreTransaction transaction);

        /// <summary>
        /// Saves the given job record, either creating it or updating it.
        /// </summary>
        /// <param name="record">The job to save.</param>
        public virtual void SaveJob(JobRecord record)
        {
            this.SaveJob(record, null);
        }

        /// <summary>
        /// Saves the given job record, either creating it or updating it.
        /// </summary>
        /// <param name="record">The job to save.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        public abstract void SaveJob(JobRecord record, IJobStoreTransaction transaction);

        /// <summary>
        /// Starts a transaction.
        /// </summary>
        public abstract IJobStoreTransaction StartTransaction();
    }
}
