//-----------------------------------------------------------------------
// <copyright file="GeocodePostalCode.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a geocode postal code.
    /// </summary>
    [DataContract]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodePostalCode
    {
        /// <summary>
        /// Gets or sets the postal code number.
        /// </summary>
        [DataMember]
        public string PostalCodeNumber { get; set; }
    }
}
