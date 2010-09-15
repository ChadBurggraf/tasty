//-----------------------------------------------------------------------
// <copyright file="GeocodeSubAdministrativeArea.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represetns a sub-administrative area in a geocode response.
    /// </summary>
    [DataContract]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodeSubAdministrativeArea
    {
        /// <summary>
        /// Gets or sets the name of the sub-adminsitrative area.
        /// </summary>
        [DataMember]
        public string SubAdministrativeAreaName { get; set; }

        /// <summary>
        /// Gets or sets the locality of the sub-administrative area.
        /// </summary>
        [DataMember]
        public GeocodeLocality Locality { get; set; }
    }
}
