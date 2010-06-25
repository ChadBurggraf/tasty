//-----------------------------------------------------------------------
// <copyright file="JobRecord.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;

    /// <summary>
    /// Represents a job record in persistent storage.
    /// </summary>
    [Serializable]
    public sealed class JobRecord
    {
        /// <summary>
        /// Initializes a new instance of the JobRecord class.
        /// </summary>
        public JobRecord()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the JobRecord class.
        /// </summary>
        /// <param name="record">The prototype <see cref="JobRecord"/> to initialize this instance from.</param>
        public JobRecord(JobRecord record)
        {
            if (record != null)
            {
                this.Data = record.Data;
                this.Exception = record.Exception;
                this.FinishDate = record.FinishDate;
                this.Id = record.Id;
                this.JobType = record.JobType;
                this.Name = record.Name;
                this.QueueDate = record.QueueDate;
                this.ScheduleName = record.ScheduleName;
                this.StartDate = record.StartDate;
                this.Status = record.Status;
            }
        }

        /// <summary>
        /// Gets or sets the serialized job data (i.e., from calling <see cref="IJob.Serialize"/>.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Gets or sets the exeption that occurred during job execution.
        /// This property can be set by wrapping the exception in an <see cref="ExceptionXElement"/>
        /// and calling ToString().
        /// </summary>
        public string Exception { get; set; }

        /// <summary>
        /// Gets or sets the date the job finished, no matter the final status.
        /// </summary>
        public DateTime? FinishDate { get; set; }

        /// <summary>
        /// Gets or sets the job ID. Use null for a new record.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IJob"/> implementor that the job is persisted for.
        /// </summary>
        public string JobType { get; set; }

        /// <summary>
        /// Gets or sets the job's display name (i.e., the value of <see cref="IJob.Name"/>).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the date the job is queued for.
        /// </summary>
        public DateTime QueueDate { get; set; }

        /// <summary>
        /// Gets or sets the name the schedule the job is queued for, if applicable.
        /// </summary>
        public string ScheduleName { get; set; }

        /// <summary>
        /// Gets or sets the date the job started execution.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the job's status.
        /// </summary>
        public JobStatus Status { get; set; }
    }
}
