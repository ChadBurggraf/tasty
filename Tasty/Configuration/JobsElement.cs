//-----------------------------------------------------------------------
// <copyright file="JobsElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents the jobs configuration element.
    /// </summary>
    public class JobsElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the heartbeat timeout (in miliseconds) to use for the job runner. The runner will 
        /// pause for this duration at the end of each cancel/finish/timeout/dequeue loop.
        /// When not configured, defaults to 10,000 (10 seconds).
        /// </summary>
        [ConfigurationProperty("heartbeat", IsRequired = false)]
        public int Heartbeat
        {
            get { return (int)(this["heartbeat"] ?? (this["heartbeat"] = 10000)); }
        }

        /// <summary>
        /// Gets a value indicating whether the job runner should be executed in
        /// the web process. When not configured, defaults to true.
        /// </summary>
        [ConfigurationProperty("inProcess", IsRequired = false)]
        public bool InProcess
        {
            get { return (bool)(this["inProcess"] ?? (this["inProcess"] = true)); }
        }

        /// <summary>
        /// Gets the maximum number of jobs that are allowed to be
        /// running simultaneously. When not configured, defaults to 10.
        /// </summary>
        [ConfigurationProperty("maximumConcurrency", IsRequired = false)]
        public int MaximumConcurrency
        {
            get { return (int)(this["maximumConcurrency"] ?? (this["maximumConcurrency"] = 10)); }
        }

        /// <summary>
        /// Gets the maximum number of retries to perform when a job
        /// fails or is timed out. When not configured, defaults to 0 (no retries).
        /// </summary>
        [ConfigurationProperty("maximumFailedRetries", IsRequired = false)]
        public int MaximumFailedRetries
        {
            get { return (int)(this["maximumFailedRetries"] ?? (this["maximumFailedRetries"] = 0)); }
        }

        /// <summary>
        /// Gets the configured collection of scheduled jobs.
        /// </summary>
        [ConfigurationProperty("schedules", IsRequired = false)]
        public JobScheduleElementCollection Schedules
        {
            get { return (JobScheduleElementCollection)(this["schedules"] ?? (this["schedules"] = new JobScheduleElementCollection())); }
        }
    }
}
