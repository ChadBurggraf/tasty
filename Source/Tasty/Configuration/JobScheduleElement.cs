//-----------------------------------------------------------------------
// <copyright file="JobScheduleElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents a job schedule in the configuration.
    /// </summary>
    public class JobScheduleElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the schedule's name.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        /// Gets or sets the repeat type the schedule uses.
        /// </summary>
        [ConfigurationProperty("repeat", IsRequired = true)]
        public JobScheduleRepeatType Repeat
        {
            get { return (JobScheduleRepeatType)this["repeat"]; }
            set { this["repeat"] = value; }
        }

        /// <summary>
        /// Gets the jobs that are configured to run as part of this schedule.
        /// </summary>
        [ConfigurationProperty("scheduledJobs", IsRequired = false)]
        public JobScheduledJobElementCollection ScheduledJobs
        {
            get { return (JobScheduledJobElementCollection)(this["scheduledJobs"] ?? (this["scheduledJobs"] = new JobScheduledJobElementCollection())); }
        }

        /// <summary>
        /// Gets or sets the date and time the schedule starts on.
        /// </summary>
        [ConfigurationProperty("startOn", IsRequired = true)]
        public DateTime StartOn
        {
            get { return (DateTime)this["startOn"]; }
            set { this["startOn"] = value; }
        }
    }
}