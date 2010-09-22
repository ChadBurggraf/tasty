//-----------------------------------------------------------------------
// <copyright file="GitHubElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents the GitHub element in the configuration.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GitHubElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the webhook collection.
        /// </summary>
        [ConfigurationProperty("webhooks", IsRequired = false, IsDefaultCollection = true)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public WebhookElementCollection Webhooks
        {
            get { return (WebhookElementCollection)(this["webhooks"] ?? (this["webhooks"] = new WebhookElementCollection())); }
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
