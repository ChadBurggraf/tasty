//-----------------------------------------------------------------------
// <copyright file="JobScheduledJobElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Prepresents a configured scheduled job.
    /// </summary>
    public class JobScheduledJobElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the scheduled job's type.
        /// </summary>
        [ConfigurationProperty("jobType", IsRequired = true, IsKey = true)]
        public string JobType
        {
            get { return (string)this["jobType"]; }
        }

        /// <summary>
        /// Gets the scheduled job's override for the maximum number of times
        /// a failed execution is re-tried.
        /// </summary>
        [ConfigurationProperty("maximumFailedRetries", IsRequired = false)]
        public int MaximumFailedRetries
        {
            get { return (int)(this["maximumFailedRetries"] ?? 3); }
        }

        /// <summary>
        /// Gets any metadata configured for the scheduled job.
        /// </summary>
        [ConfigurationProperty("metadata", IsRequired = false)]
        public KeyValueConfigurationCollection Metadata
        {
            get { return (KeyValueConfigurationCollection)(this["metadata"] ?? (this["metadata"] = new KeyValueConfigurationCollection())); }
        }

        /// <summary>
        /// Gets the repeat type the schedule uses.
        /// </summary>
        [ConfigurationProperty("repeat", IsRequired = true)]
        public JobScheduleRepeatType Repeat
        {
            get { return (JobScheduleRepeatType)this["repeat"]; }
        }
    }
}