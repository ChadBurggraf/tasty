//-----------------------------------------------------------------------
// <copyright file="GeocodePlacemark.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a placemark in a geocode response.
    /// </summary>
    [DataContract]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodePlacemark
    {
        /// <summary>
        /// Gets or sets the address string of the response.
        /// </summary>
        [DataMember(Name = "address")]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the address details of the response.
        /// </summary>
        [DataMember]
        public GeocodeAddressDetails AddressDetails { get; set; }

        /// <summary>
        /// Gets or sets the extended data of the response.
        /// </summary>
        [DataMember]
        public GeocodeExtendedData ExtendedData { get; set; }

        /// <summary>
        /// Gets or sets the response ID.
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the response's location point.
        /// </summary>
        [DataMember]
        public GeocodePoint Point { get; set; }
    }
}
