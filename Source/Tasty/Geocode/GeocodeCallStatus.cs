//-----------------------------------------------------------------------
// <copyright file="GeocodeCallStatus.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines the possible status results of an encapsulated geocode request/response call.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public enum GeocodeCallStatus
    {
        /// <summary>
        /// Identifies that the call was unsuccessful due to an exception or bad status code.
        /// </summary>
        Unsuccessful = 0,

        /// <summary>
        /// Identifies that the response didn't have enough accuracy.
        /// </summary>
        NotEnoughAccuracy,

        /// <summary>
        /// Identifies that the call was successful.
        /// </summary>
        Successful
    }
}
