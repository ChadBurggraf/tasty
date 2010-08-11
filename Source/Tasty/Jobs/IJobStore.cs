//-----------------------------------------------------------------------
// <copyright file="IJobStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using Tasty.Configuration;

    /// <summary>
    /// Defines the interface for persistent job stores.
    /// </summary>
    public interface IJobStore : IDisposable
    {
        /// <summary>
        /// Commits the currently in-progress transaction.
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Deletes a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to delete.</param>
        void DeleteJob(int id);

        /// <summary>
        /// Gets a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to get.</param>
        /// <returns>The job with the given ID.</returns>
        JobRecord GetJob(int id);

        /// <summary>
        /// Gets a collection of jobs that match the given collection of IDs.
        /// </summary>
        /// <param name="ids">The IDs of the jobs to get.</param>
        /// <returns>A collection of jobs.</returns>
        IEnumerable<JobRecord> GetJobs(IEnumerable<int> ids);

        /// <summary>
        /// Gets a collection of jobs with the given status, returning
        /// at most the number of jobs identified by <paramref name="count"/>.
        /// </summary>
        /// <param name="status">The status of the jobs to get.</param>
        /// <param name="count">The maximum number of jobs to get.</param>
        /// <returns>A collection of jobs.</returns>
        IEnumerable<JobRecord> GetJobs(JobStatus status, int count);

        /// <summary>
        /// Gets a collection of the most recently scheduled persisted job for each
        /// scheduled job in the configuration.
        /// </summary>
        /// <returns>A collection of recently scheduled jobs.</returns>
        IEnumerable<JobRecord> GetLatestScheduledJobs();

        /// <summary>
        /// Initializes the job store with the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration to initialize the job store with.</param>
        void Initialize(TastySettings configuration);

        /// <summary>
        /// Rolls back the currently in-progress transaction.
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Saves the given job record, either creating it or updating it.
        /// </summary>
        /// <param name="record">The job to save.</param>
        void SaveJob(JobRecord record);

        /// <summary>
        /// Starts a transaction.
        /// </summary>
        void StartTransaction();
    }
}
