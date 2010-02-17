//-----------------------------------------------------------------------
// <copyright file="GeocodeThoroughfare.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a throroughfare in a geocode response.
    /// </summary>
    [DataContract]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodeThoroughfare
    {
        /// <summary>
        /// Gets or sets the name of the thoroughfare.
        /// </summary>
        [DataMember]
        public string ThoroughfareName { get; set; }
    }
}
