//-----------------------------------------------------------------------
// <copyright file="JobStoreElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents a configuration element describing the current <see cref="Tasty.Jobs.IJobStore"/> being used.
    /// </summary>
    public class JobStoreElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the type of the <see cref="Tasty.Jobs.IJobStore"/> implementation to use when persisting jobs.
        /// </summary>
        [ConfigurationProperty("type", IsRequired = false, DefaultValue = "Tasty.Jobs.SqlServerJobStore, Tasty")]
        public string JobStoreType
        {
            get { return (string)this["type"]; }
        }

        /// <summary>
        /// Gets any metadata configured for job store.
        /// </summary>
        [ConfigurationProperty("metadata", IsRequired = false)]
        public KeyValueConfigurationCollection Metadata
        {
            get { return (KeyValueConfigurationCollection)(this["metadata"] ?? (this["metadata"] = new KeyValueConfigurationCollection())); }
        }
    }
}
