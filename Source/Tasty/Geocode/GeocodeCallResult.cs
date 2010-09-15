//-----------------------------------------------------------------------
// <copyright file="GeocodeCallResult.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents the result of an encapsulated geocode request/response call.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodeCallResult
    {
        /// <summary>
        /// Gets or sets the placemark that was returned in the response.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The casing is consistent with the API.")]
        public GeocodePlacemark Placemark { get; set; }

        /// <summary>
        /// Gets or sets the result status of the call.
        /// </summary>
        public GeocodeCallStatus Status { get; set; }
    }
}
