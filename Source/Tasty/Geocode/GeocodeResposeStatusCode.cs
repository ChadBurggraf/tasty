//-----------------------------------------------------------------------
// <copyright file="GeocodeResposeStatusCode.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines the possible <see cref="Tasty.Geocode.GeocodeResponseStatus"/> codes.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public enum GeocodeResposeStatusCode
    {
        /// <summary>
        /// Identifies an unknown status code in a response.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Identifies a successful response.
        /// </summary>
        Success = 200,

        /// <summary>
        /// Identifies a server error.
        /// </summary>
        ServerError = 500,

        /// <summary>
        /// Identifies an error due to a missing query.
        /// </summary>
        MissingQuery = 601,

        /// <summary>
        /// Identifies an error due to an unknown address.
        /// </summary>
        UnknownAddress = 602,

        /// <summary>
        /// Identifies an error due to an unavailable address.
        /// </summary>
        UnavailableAddress = 603,

        /// <summary>
        /// Identifies an error due to a bad API key.
        /// </summary>
        BadKey = 610,

        /// <summary>
        /// Identifies an error due to too many requests.
        /// </summary>
        TooManyQueries = 620
    }
}
