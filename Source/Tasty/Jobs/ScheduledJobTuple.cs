//-----------------------------------------------------------------------
// <copyright file="ScheduledJobTuple.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using Tasty.Configuration;

    /// <summary>
    /// Represents an expanded tuple of information about an individual scheduled job.
    /// </summary>
    public class ScheduledJobTuple
    {
        /// <summary>
        /// Initializes a new instance of the ScheduledJobTuple class.
        /// </summary>
        public ScheduledJobTuple()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ScheduledJobTuple from the given instance.
        /// Does not copy over the <see cref="ScheduledJobTuple.Record"/> property.
        /// </summary>
        /// <param name="tuple">The tuple instance to create this instance from.</param>
        /// <param name="record">The new tuple's <see cref="JobRecord"/>.</param>
        public ScheduledJobTuple(ScheduledJobTuple tuple, JobRecord record)
        {
            if (tuple != null)
            {
                this.Schedule = tuple.Schedule;
                this.ScheduledJob = tuple.ScheduledJob;
            }

            this.Record = record;
        }

        /// <summary>
        /// Gets or sets scheduled job's previous run record.
        /// </summary>
        public JobRecord Record { get; set; }

        /// <summary>
        /// Gets or sets the scheduled job's owner schedule.
        /// </summary>
        public JobScheduleElement Schedule { get; set; }

        /// <summary>
        /// Gets or sets the scheduled job's definition.
        /// </summary>
        public JobScheduledJobElement ScheduledJob { get; set; }
    }
}
