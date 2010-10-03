//-----------------------------------------------------------------------
// <copyright file="TimeZoneCallStatus.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
//     Adapted from code by Jason Sukut, copyright (c) 2010 Jason Sukut.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines the possible <see cref="TimeZoneResponse"/> status codes.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "There is no real world default value, and it is defaulted properly any place it is instantiated.")]
    public enum TimeZoneCallStatus
    {
        /// <summary>
        /// Indicates that the request was not authorized.
        /// </summary>
        AuthorizationException = 10,

        /// <summary>
        /// Indicates an unkown error caused the request to fail.
        /// </summary>
        Unknown = 12,

        /// <summary>
        /// Indicates that the database reuqest timed out.
        /// </summary>
        Timeout = 13,

        /// <summary>
        /// Indicates that the latitude or longitude parameters are invalid
        /// </summary>
        InvalidParameter = 14,

        /// <summary>
        /// Indicates that a time zone could not be found for the given coordinates
        /// </summary>
        NotFound = 15,

        /// <summary>
        /// Indicates a successful request.
        /// </summary>
        Success = 200
    }
}
