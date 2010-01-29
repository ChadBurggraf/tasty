//-----------------------------------------------------------------------
// <copyright file="IJobStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Interface definition for persistent job stores.
    /// </summary>
    public interface IJobStore
    {
        /// <summary>
        /// Creates a new job record.
        /// </summary>
        /// <param name="record">The record to create.</param>
        /// <returns>The created record.</returns>
        JobRecord CreateJob(JobRecord record);

        /// <summary>
        /// Gets a collection of queued jobs that can be dequeued right now.
        /// </summary>
        /// <returns>A collection of queued jobs.</returns>
        IEnumerable<JobRecord> DequeueJobs();

        /// <summary>
        /// Gets a collection of jobs that have been marked as <see cref="JobStatus.Canceling"/>.
        /// </summary>
        /// <returns>A collection of cancelling jobs.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This operation may be expensive.")]
        IEnumerable<JobRecord> GetCancellingJobs();

        /// <summary>
        /// Updates a collection of jobs within a transaction through the execution
        /// of the given delegate.
        /// </summary>
        /// <param name="records">The records to update.</param>
        /// <param name="onIterate">The delegate that will perform property updates (but not persistence) on each job record.
        /// Pass null to update the job store directly with the given record values.</param>
        void UpdateJobs(IEnumerable<JobRecord> records, Action<JobRecord> onIterate);
    }
}