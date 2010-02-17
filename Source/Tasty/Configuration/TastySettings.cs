//-----------------------------------------------------------------------
// <copyright file="TastySettings.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents the Tasty.dll configuration section.
    /// </summary>
    public class TastySettings : ConfigurationSection
    {
        private static TastySettings section = (TastySettings)(ConfigurationManager.GetSection("tasty") ?? new TastySettings());

        /// <summary>
        /// Gets the Tasty.dll configuration section.
        /// </summary>
        public static TastySettings Section
        {
            get { return section; }
        }

        /// <summary>
        /// Gets the geocode configuration element.
        /// </summary>
        [ConfigurationProperty("geocode", IsRequired = false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public GeocodeElement Geocode
        {
            get { return (GeocodeElement)(this["geocode"] ?? (this["geocode"] = new GeocodeElement())); }
        }

        /// <summary>
        /// Gets the jobs configuration element.
        /// </summary>
        [ConfigurationProperty("jobs", IsRequired = false)]
        public JobsElement Jobs
        {
            get { return (JobsElement)(this["jobs"] ?? (this["jobs"] = new JobsElement())); }
        }

        /// <summary>
        /// Gets the HTTP configuration element.
        /// </summary>
        [ConfigurationProperty("http", IsRequired = false)]
        public HttpElement Http
        {
            get { return (HttpElement)(this["http"] ?? (this["http"] = new HttpElement())); }
        }

        /// <summary>
        /// Gets the URL tokens configuration element.
        /// </summary>
        [ConfigurationProperty("urlTokens", IsRequired = false)]
        public UrlTokensElement UrlTokens
        {
            get { return (UrlTokensElement)(this["urlTokens"] ?? (this["urlTokens"] = new UrlTokensElement())); }
        }

        /// <summary>
        /// Gets a connection string value from the connection string name identified in the given
        /// configured metadata collection.
        /// </summary>
        /// <param name="metadata">The metadata configuration collection containing the connection string name.</param>
        /// <returns>The connection string value, or null if not found.</returns>
        public static string GetConnectionStringFromMetadata(KeyValueConfigurationCollection metadata)
        {
            KeyValueConfigurationElement keyValueElement = metadata["ConnectionStringName"];
            string name = keyValueElement != null ? keyValueElement.Value : "LocalSqlServer";
            string connectionString = null;

            if (!String.IsNullOrEmpty(name))
            {
                ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[name];

                if (connectionStringSettings != null)
                {
                    connectionString = connectionStringSettings.ConnectionString;
                }
                else
                {
                    var appSetting = ConfigurationManager.AppSettings[name];

                    if (!String.IsNullOrEmpty(appSetting))
                    {
                        connectionString = appSetting;
                    }
                }
            }

            return connectionString;
        }
    }
}
