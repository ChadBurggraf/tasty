//-----------------------------------------------------------------------
// <copyright file="UrlTokensElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using Tasty.Web.UrlTokens;

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
        /// Gets any metadata configured for the URL token store.
        /// </summary>
        [ConfigurationProperty("metadata", IsRequired = false)]
        public KeyValueConfigurationCollection Metadata
        {
            get { return (KeyValueConfigurationCollection)(this["metadata"] ?? (this["metadata"] = new KeyValueConfigurationCollection())); }
        }

        /// <summary>
        /// Gets or sets the <see cref="IUrlTokenStore"/> implementation to use when persisting URL tokens.
        /// </summary>
        [ConfigurationProperty("storeType", IsRequired = false, DefaultValue = "Tasty.Web.UrlTokens.HttpCacheUrlTokenStore, Tasty")]
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Not a URL property.")]
        public string UrlTokenStoreType
        {
            get { return (string)this["storeType"]; }
            set { this["storeType"] = value; }
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
