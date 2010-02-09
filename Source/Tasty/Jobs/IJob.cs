﻿//-----------------------------------------------------------------------
// <copyright file="IJob.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;

    /// <summary>
    /// Defines the interface for background jobs.
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Gets the job's display name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the timeout, in miliseconds, the job is allowed to run for.
        /// </summary>
        long Timeout { get; }

        /// <summary>
        /// Enqueues the job for execution.
        /// </summary>
        /// <returns>The job record that was persisted.</returns>
        JobRecord Enqueue();

        /// <summary>
        /// Enqueues the job for execution on a certin date and for a specific schedule.
        /// </summary>
        /// <param name="queueDate">The date to queue the job for execution on.</param>
        /// <param name="scheduleName">The name of the schedule to queue the job for, or null if not applicable.</param>
        /// <returns>The job record that was persisted.</returns>
        JobRecord Enqueue(DateTime queueDate, string scheduleName);

        /// <summary>
        /// Executes the job.
        /// </summary>
        void Execute();

        /// <summary>
        /// Serializes the job state for enqueueing.
        /// </summary>
        /// <returns>The serialized job data.</returns>
        string Serialize();
    }
}