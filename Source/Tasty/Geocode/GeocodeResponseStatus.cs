//-----------------------------------------------------------------------
// <copyright file="GeocodeResponseStatus.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a geocode response status.
    /// </summary>
    [DataContract]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodeResponseStatus
    {
        /// <summary>
        /// Gets or sets the status code of the response.
        /// </summary>
        [DataMember(Name = "code")]
        public GeocodeResposeStatusCode Code { get; set; }

        /// <summary>
        /// Gets or sets the type of request that generated the response.
        /// </summary>
        [DataMember(Name = "request")]
        public string Request { get; set; }
    }
}
