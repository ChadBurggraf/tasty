//-----------------------------------------------------------------------
// <copyright file="TimeZoneCallResult.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
//     Adapted from code by Jason Sukut, copyright (c) 2010 Jason Sukut.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;

    /// <summary>
    /// Represents the result of an encapsulated timezone request/response call.
    /// </summary>
    public class TimeZoneCallResult
    {
        /// <summary>
        /// Gets or sets the status of the call.
        /// </summary>
        public TimeZoneCallStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the timezone that was returned in the response.
        /// </summary>
        public string TimeZone { get; set; }
    }
}
