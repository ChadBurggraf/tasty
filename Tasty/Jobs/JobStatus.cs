//-----------------------------------------------------------------------
// <copyright file="JobStatus.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System.ComponentModel;

    /// <summary>
    /// Defies the possible job status types.
    /// </summary>
    public enum JobStatus
    {
        /// <summary>
        /// Identifies an explicitly canceled job.
        /// </summary>
        Canceled,

        /// <summary>
        /// Identifies a job that is in the process of being canceled.
        /// </summary>
        Canceling,

        /// <summary>
        /// Identifies a failed job.
        /// </summary>
        Failed,

        /// <summary>
        /// Identifies a queued job.
        /// </summary>
        Queued,

        /// <summary>
        /// Identifies a job that has started.
        /// </summary>
        Started,

        /// <summary>
        /// Identifies a job that succeeded.
        /// </summary>
        Succeeded,

        /// <summary>
        /// Identifies a job that has timed out.
        /// </summary>
        [Description("Timed Out")]
        TimedOut
    }
}