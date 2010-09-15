//-----------------------------------------------------------------------
// <copyright file="HttpElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents the HTTP configuration element.
    /// </summary>
    public class HttpElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the configured HTTP redirects collection.
        /// </summary>
        [ConfigurationProperty("redirects", IsRequired = false)]
        public HttpRedirectRuleElementCollection Redirects
        {
            get { return (HttpRedirectRuleElementCollection)(this["redirects"] ?? (this["redirects"] = new HttpRedirectRuleElementCollection())); }
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