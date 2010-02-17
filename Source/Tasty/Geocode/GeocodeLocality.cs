//-----------------------------------------------------------------------
// <copyright file="GeocodeLocality.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a locality in a geocode response.
    /// </summary>
    [DataContract]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodeLocality
    {
        /// <summary>
        /// Gets or sets the name of the locality.
        /// </summary>
        [DataMember]
        public string LocalityName { get; set; }

        /// <summary>
        /// Gets or sets the thoroughfare of the locality.
        /// </summary>
        [DataMember]
        public GeocodeThoroughfare Thoroughfare { get; set; }

        /// <summary>
        /// Gets or sets the postal code of the locality.
        /// </summary>
        [DataMember]
        public GeocodePostalCode PostalCode { get; set; }
    }
}
