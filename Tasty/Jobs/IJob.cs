//-----------------------------------------------------------------------
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
        /// Serializes the job state for enqueueing.
        /// </summary>
        /// <returns>The serialized job data.</returns>
        string Serialize();
    }
}