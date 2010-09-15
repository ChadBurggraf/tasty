//-----------------------------------------------------------------------
// <copyright file="GeocodeExtendedData.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents extended data in a geocode response.
    /// </summary>
    [DataContract]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodeExtendedData
    {
        /// <summary>
        /// Gets or sets the latitude/longitude bounding box of the geocode response.
        /// </summary>
        [DataMember]
        public GeocodeLatLonBox LatLonBox { get; set; }
    }
}
