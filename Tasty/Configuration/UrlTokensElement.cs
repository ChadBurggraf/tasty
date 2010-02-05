//-----------------------------------------------------------------------
// <copyright file="UrlTokens.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Configuration;
    using Tasty.Web;

    /// <summary>
    /// Represents the URL tokens configuration element.
    /// </summary>
    public class UrlTokensElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the default number of hours after creation URL tokens expire.
        /// When not configured, defaults to 168 (1 week).
        /// </summary>
        [ConfigurationProperty("defaultExpiryHours", IsRequired = false, DefaultValue = 168)]
        public int DefaultExpiryHours
        {
            get { return (int)this["defaultExpiryHours"]; }
            set { this["defaultExpiryHours"] = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="IUrlTokenStore"/> implementation to use when persisting URL tokens.
        /// </summary>
        [ConfigurationProperty("storeType", IsRequired = false, DefaultValue = "Tasty.Web.SqlUrlTokenStore, Tasty")]
        public string UrlTokenStoreType
        {
            get { return (string)this["storeType"]; }
            set { this["storeType"] = value; }
        }
    }
}
