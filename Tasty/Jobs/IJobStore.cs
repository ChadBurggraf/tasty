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
        /// Gets a collection of jobs that have been marked as <see cref="JobStatus.Canceling"/>.
        /// Opens a new transaction, then calls the delegate to perform any work. The transaction
        /// is committed when the delegate returns.
        /// </summary>
        /// <param name="canceling">The function to call with the canceling job collection.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "I'm not strongly typing a delegate when this will work just fine.")]
        void CancelingJobs(Action<IEnumerable<JobRecord>> canceling);

        /// <summary>
        /// Creates a new job record.
        /// </summary>
        /// <param name="record">The record to create.</param>
        /// <returns>The created record.</returns>
        JobRecord CreateJob(JobRecord record);

        /// <summary>
        /// Gets a collection of queued jobs that can be dequeued right now.
        /// Opens a new transaction, then calls the delegate to perform any work. The transaction
        /// is committed when the delegate returns.
        /// </summary>
        /// <param name="dequeueing">The function to call with the dequeued job collection.</param>
        /// <param name="runsAvailable">The maximum number of job job runs currently available, as determined by
        /// the <see cref="Tasty.Configuration.JobsElement.MaximumConcurrency"/> - the number of currently running jobs.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "I'd be happy to learn of an alternative spelling.")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "I'm not strongly typing a delegate when this will work just fine.")]
        void DequeueJobs(Action<IEnumerable<JobRecord>> dequeueing, int runsAvailable);

        /// <summary>
        /// Updates a collection of jobs. Opens a new transaction, then calls the delegate to perform
        /// any work on each record. The transaction is committed when all of the records have been iterated through.
        /// </summary>
        /// <param name="records">The records to update.</param>
        /// <param name="updating">The function to call for each iteration, which should perform any updates necessary on the job record.</param>
        void UpdateJobs(IEnumerable<JobRecord> records, Action<JobRecord> updating);
    }
}