//-----------------------------------------------------------------------
// <copyright file="HttpElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
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
    }
}