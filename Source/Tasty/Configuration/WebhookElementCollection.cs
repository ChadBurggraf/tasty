//-----------------------------------------------------------------------
// <copyright file="WebhookElementCollection.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Represents a collection of <see cref="WebhookElement"/>s in the configuration.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class WebhookElementCollection : ConfigurationElementCollection<WebhookElement>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to stifle all exceptions. Useful for preventing
        /// leakage of any sensitive information.
        /// </summary>
        [ConfigurationProperty("stifleExceptions", IsRequired = false, DefaultValue = true)]
        public bool StifleExceptions
        {
            get { return (bool)this["stifleExceptions"]; }
            set { this["stifleExceptions"] = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the collection contains the given item.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns>True if the collection contains the item, false otherwise.</returns>
        public override bool Contains(WebhookElement item)
        {
            return this.Any(e => e.Repository.Equals(item.Repository, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the unique key of the given element.
        /// </summary>
        /// <param name="element">The element to get the key of.</param>
        /// <returns>The given element's key.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WebhookElement)element).Repository;
        }
    }
}
