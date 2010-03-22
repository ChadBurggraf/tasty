﻿//-----------------------------------------------------------------------
// <copyright file="GeocodeAdministrativeArea.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represetns an administrative area in a geocode response.
    /// </summary>
    [DataContract]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodeAdministrativeArea
    {
        /// <summary>
        /// Gets or sets the name of the administrative area.
        /// </summary>
        [DataMember]
        public string AdministrativeAreaName { get; set; }

        /// <summary>
        /// Gets or sets the sub-administraive area.
        /// </summary>
        [DataMember]
        public GeocodeSubAdministrativeArea SubAdministrativeArea { get; set; }
    }
}