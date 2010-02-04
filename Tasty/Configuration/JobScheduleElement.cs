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
        /// Gets a value indicating whether the schedule is enabled.
        /// </summary>
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
        }

        /// <summary>
        /// Gets the schedule's name.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
        }

        /// <summary>
        /// Gets the date and time the schedule starts on.
        /// </summary>
        [ConfigurationProperty("startOn", IsRequired = true)]
        public DateTime StartOn
        {
            get { return (DateTime)this["startOn"]; }
        }
    }
}