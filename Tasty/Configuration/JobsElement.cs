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
        [ConfigurationProperty("heartbeat", IsRequired = false, DefaultValue = 10000)]
        public int Heartbeat
        {
            get { return (int)this["heartbeat"]; }
        }

        /// <summary>
        /// Gets the maximum number of jobs that are allowed to be
        /// running simultaneously. When not configured, defaults to 10.
        /// </summary>
        [ConfigurationProperty("maximumConcurrency", IsRequired = false, DefaultValue = 10)]
        public int MaximumConcurrency
        {
            get { return (int)this["maximumConcurrency"]; }
        }

        /// <summary>
        /// Gets the maximum number of retries to perform when a job
        /// fails or is timed out. When not configured, defaults to 0 (no retries).
        /// </summary>
        [ConfigurationProperty("maximumFailedRetries", IsRequired = false)]
        public int MaximumFailedRetries
        {
            get { return (int)this["maximumFailedRetries"]; }
        }

        /// <summary>
        /// Gets the configured collection of scheduled jobs.
        /// </summary>
        [ConfigurationProperty("schedules", IsRequired = false)]
        public JobScheduleElementCollection Schedules
        {
            get { return (JobScheduleElementCollection)(this["schedules"] ?? (this["schedules"] = new JobScheduleElementCollection())); }
        }

        /// <summary>
        /// Gets the configured <see cref="Tasty.Jobs.IJobStore"/> implementation to use for persisting job data.
        /// </summary>
        [ConfigurationProperty("store", IsRequired = false)]
        public JobStoreElement Store
        {
            get { return (JobStoreElement)(this["store"] ?? (this["store"] = new JobStoreElement())); }
        }
    }
}
