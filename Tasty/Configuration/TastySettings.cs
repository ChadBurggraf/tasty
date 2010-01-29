//-----------------------------------------------------------------------
// <copyright file="TastySettings.cs" company="Chad Burggraf">
//     Copyright (c) 2010 Chad Burggraf.
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
        /// Gets the HTTP configuration element.
        /// </summary>
        [ConfigurationProperty("http", IsRequired = false)]
        public HttpElement Http
        {
            get { return (HttpElement)(this["http"] ?? (this["http"] = new HttpElement())); }
        }
    }
}
