//-----------------------------------------------------------------------
// <copyright file="GeocodeLatLonBox.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represetns a latitude/longitude bounding box in a geocode response.
    /// </summary>
    [DataContract]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodeLatLonBox
    {
        /// <summary>
        /// Gets or sets the eastern coordinate of the box.
        /// </summary>
        [DataMember(Name = "east")]
        public decimal? East { get; set; }

        /// <summary>
        /// Gets or sets the northern coordinate of the box.
        /// </summary>
        [DataMember(Name = "north")]
        public decimal? North { get; set; }

        /// <summary>
        /// Gets or sets the southern coordinate of the box.
        /// </summary>
        [DataMember(Name = "south")]
        public decimal? South { get; set; }

        /// <summary>
        /// Gets or sets the western corrdinate of the box.
        /// </summary>
        [DataMember(Name = "west")]
        public decimal? West { get; set; }
    }
}
