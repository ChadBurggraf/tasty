//-----------------------------------------------------------------------
// <copyright file="GeocodeCountry.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a country in a geocode response.
    /// </summary>
    [DataContract]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodeCountry
    {
        /// <summary>
        /// Gets or sets the name code of the country.
        /// </summary>
        [DataMember]
        public string CountryNameCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the country.
        /// </summary>
        [DataMember]
        public string CountryName { get; set; }

        /// <summary>
        /// Gets or sets the administrative area in the country.
        /// </summary>
        [DataMember]
        public GeocodeAdministrativeArea AdministrativeArea { get; set; }
    }
}
