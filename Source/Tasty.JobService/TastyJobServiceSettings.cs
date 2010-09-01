//-----------------------------------------------------------------------
// <copyright file="TastyJobServiceSettings.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.JobService
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents the tasty job service configuration section.
    /// </summary>
    public class TastyJobServiceSettings : ConfigurationSection
    {
        private static TastyJobServiceSettings section;

        /// <summary>
        /// Gets or sets the job service settings configuration section.
        /// </summary>
        public static TastyJobServiceSettings Section
        {
            get { return section ?? (section = (TastyJobServiceSettings)(ConfigurationManager.GetSection("tastyJobService") ?? new TastyJobServiceSettings())); }
            set { section = value; }
        }

        /// <summary>
        /// Gets the applications configuration element collection.
        /// </summary>
        [ConfigurationProperty("applications", IsDefaultCollection = true)]
        public ApplicationElementCollection Applications
        {
            get { return (ApplicationElementCollection)(this["applications"] ?? (this["applications"] = new ApplicationElementCollection())); }
        }
    }
}
