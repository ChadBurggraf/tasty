//-----------------------------------------------------------------------
// <copyright file="TastySettings.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Configuration;

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
    }
}
