//-----------------------------------------------------------------------
// <copyright file="IJobRunnerDelegate.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines the interface for job runner delegates.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Well, it is a delegate isn't it?")]
    public interface IJobRunnerDelegate
    {
        /// <summary>
        /// Called when a job is canceled.
        /// </summary>
        /// <param name="record">The job record identifying the affected job.</param>
        void OnCancelJob(JobRecord record);

        /// <summary>
        /// Called when a job is dequeued.
        /// </summary>
        /// <param name="record">The job record identifying the affected job.</param>
        void OnDequeueJob(JobRecord record);

        /// <summary>
        /// Called when a scheduled job is enqueued.
        /// </summary>
        /// <param name="record">The job record identifying the affected job.</param>
        void OnEnqueueScheduledJob(JobRecord record);

        /// <summary>
        /// Called when an error occurs during the execution of the run-loop.
        /// Does not get called when a job itself experiences an error; job-specific
        /// errors are saved in the job store with their respecitve records.
        /// </summary>
        /// <param name="record">The record on which the error occurred, if applicable.</param>
        /// <param name="ex">The exception raised, if applicable.</param>
        void OnError(JobRecord record, Exception ex);

        /// <summary>
        /// Called when a job is finished.
        /// </summary>
        /// <param name="record">The job record identifying the affected job.</param>
        void OnFinishJob(JobRecord record);

        /// <summary>
        /// Called when a job is timed out.
        /// </summary>
        /// <param name="record">The job record identifying the affected job.</param>
        void OnTimeoutJob(JobRecord record);
    }
}
