//-----------------------------------------------------------------------
// <copyright file="GeocodeAddressDetails.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents address details in a geocode response.
    /// </summary>
    [DataContract]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodeAddressDetails
    {
        /// <summary>
        /// Gets or sets the country of the address.
        /// </summary>
        [DataMember]
        public GeocodeCountry Country { get; set; }

        /// <summary>
        /// Gets or sets the accuracy of the address compared to the geocode request.
        /// </summary>
        [DataMember]
        public int Accuracy { get; set; }
    }
}
