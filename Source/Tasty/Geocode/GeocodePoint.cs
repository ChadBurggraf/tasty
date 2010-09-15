//-----------------------------------------------------------------------
// <copyright file="GeocodePoint.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a point in a geocode response.
    /// </summary>
    [DataContract]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodePoint
    {
        /// <summary>
        /// Gets or sets the point's coordinates.
        /// </summary>
        [DataMember(Name = "coordinates")]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Serialization property.")]
        public decimal[] Coordinates { get; set; }

        /// <summary>
        /// Gets the latitude value of the coordinates.
        /// </summary>
        public decimal? Latitude
        {
            get
            {
                return (this.Coordinates != null && this.Coordinates.Length > 1) ? (decimal?)this.Coordinates[1] : null;
            }
        }

        /// <summary>
        /// Gets the longitude value of the coordinates.
        /// </summary>
        public decimal? Longitude
        {
            get
            {
                return (this.Coordinates != null && this.Coordinates.Length > 0) ? (decimal?)this.Coordinates[0] : null;
            }
        }
    }
}
