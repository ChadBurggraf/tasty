//-----------------------------------------------------------------------
// <copyright file="ScheduledJobRecord.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using Tasty.Configuration;

    /// <summary>
    /// Represents a persisted record for a scheduled job along with its schedule configuration.
    /// </summary>
    public class ScheduledJobRecord
    {
        /// <summary>
        /// Gets or sets the persisted job record.
        /// </summary>
        public JobRecord Record { get; set; }

        /// <summary>
        /// Gets or sets the job's schedule.
        /// </summary>
        public JobScheduleElement Schedule { get; set; }
    }
}
