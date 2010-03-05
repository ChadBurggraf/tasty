//-----------------------------------------------------------------------
// <copyright file="GeocodeElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents the geocode configuration element.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodeElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the API key to use in geocode requests.
        /// </summary>
        [ConfigurationProperty("apiKey", IsRequired = false)]
        public string ApiKey
        {
            get { return (string)this["apiKey"]; }
            set { this["apiKey"] = value; }
        }
    }
}
