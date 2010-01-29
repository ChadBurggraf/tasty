//-----------------------------------------------------------------------
// <copyright file="JobScheduleRepeatType.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    /// <summary>
    /// Defines the possible repeat types for job schedules.
    /// </summary>
    public enum JobScheduleRepeatType
    {
        /// <summary>
        /// Identifies that a schedule repeats daily.
        /// </summary>
        Daily = 0, // Make daily the default.

        /// <summary>
        /// Identifies that a schedule repeats hourly.
        /// </summary>
        Hourly,

        /// <summary>
        /// Identifies that a schedule repeats weekly.
        /// </summary>
        Weekly
    }
}