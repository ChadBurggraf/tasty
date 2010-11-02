//-----------------------------------------------------------------------
// <copyright file="ScheduledJobTuple.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
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
            : this(null, null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ScheduledJobTuple class from the given instance.
        /// Does not copy over the <see cref="ScheduledJobTuple.Record"/> property.
        /// </summary>
        /// <param name="tuple">The tuple instance to create this instance from.</param>
        /// <param name="record">The new tuple's <see cref="JobRecord"/>.</param>
        /// <param name="now">The current date, used to calculate whether the tuple should be executed.</param>
        /// <param name="heartbeat">The heartbeat window of the job runner, used to calculate whether the tuple should be executed.</param>
        public ScheduledJobTuple(ScheduledJobTuple tuple, JobRecord record, DateTime? now, long? heartbeat)
        {
            if (tuple != null)
            {
                this.Schedule = tuple.Schedule;
                this.ScheduledJob = tuple.ScheduledJob;

                if (now != null && heartbeat != null)
                {
                    if (now.Value.Kind != DateTimeKind.Utc)
                    {
                        throw new ArgumentException("now must be in UTC.", "now");
                    }

                    if (heartbeat <= 0)
                    {
                        throw new ArgumentException("heartbeat must be greater than 0.", "heartbeat");
                    }

                    this.ShouldExecute = Tasty.Jobs.ScheduledJob.ShouldExecute(tuple.Schedule, heartbeat.Value, now.Value);
                }
            }

            this.Record = record;

            if (record != null)
            {
                this.LastExecuteDate = record.QueueDate;
            }
        }

        /// <summary>
        /// Gets or sets the scheduled job's last execute date.
        /// </summary>
        public DateTime LastExecuteDate { get; set; }

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

        /// <summary>
        /// Gets or sets a value indicating whether the tuple should be executed.
        /// </summary>
        public bool ShouldExecute { get; set; }
    }
}
