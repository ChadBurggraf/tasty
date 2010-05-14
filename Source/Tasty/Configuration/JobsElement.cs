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
        /// Gets or sets a value indicating whether to delete job records found in the job store
        /// for scheduled jobs that could not be instantiated (e.g., if the type no longer exists or could not be loaded).
        /// </summary>
        [ConfigurationProperty("deleteBadScheduledJobRecords", IsRequired  = false, DefaultValue = false)]
        public bool DeleteBadScheduledJobRecords
        {
            get { return (bool)this["deleteBadScheduledJobRecords"]; }
            set { this["deleteBadScheduledJobRecords"] = value; }
        }

        /// <summary>
        /// Gets or sets the heartbeat timeout (in miliseconds) to use for the job runner. The runner will 
        /// pause for this duration at the end of each cancel/finish/timeout/dequeue loop.
        /// When not configured, defaults to 10,000 (10 seconds).
        /// </summary>
        [ConfigurationProperty("heartbeat", IsRequired = false, DefaultValue = 10000)]
        public int Heartbeat
        {
            get { return (int)this["heartbeat"]; }
            set { this["heartbeat"] = value; }
        }

        /// <summary>
        /// Gets or sets the maximum number of jobs that are allowed to be
        /// running simultaneously. When not configured, defaults to 4.
        /// </summary>
        [ConfigurationProperty("maximumConcurrency", IsRequired = false, DefaultValue = 4)]
        public int MaximumConcurrency
        {
            get { return (int)this["maximumConcurrency"]; }
            set { this["maximumConcurrency"] = value; }
        }

        /// <summary>
        /// Gets or sets the maximum number of retries to perform when a job
        /// fails or is timed out. When not configured, defaults to 0 (no retries).
        /// </summary>
        [ConfigurationProperty("maximumFailedRetries", IsRequired = false)]
        public int MaximumFailedRetries
        {
            get { return (int)this["maximumFailedRetries"]; }
            set { this["maximumFailedRetries"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to notify the delegate when bad
        /// scheduled job records are found in the job store (e.g., if the type no longer exists or could not be loaded).
        /// </summary>
        [ConfigurationProperty("notifyOnBadScheduledJobs", IsRequired = false, DefaultValue = false)]
        public bool NotifyOnBadScheduledJobs
        {
            get { return (bool)this["notifyOnBadScheduledJobs"]; }
            set { this["notifyOnBadScheduledJobs"] = value; }
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

        /// <summary>
        /// Gets a value indicating if the System.Configuration.ConfigurationElement object is read-only.
        /// </summary>
        /// <returns>True if the System.Configuration.ConfigurationElement object is read-only, false otherwise.</returns>
        public override bool IsReadOnly()
        {
            return false;
        }
    }
}
