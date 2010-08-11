//-----------------------------------------------------------------------
// <copyright file="Job.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;
    using Tasty.Configuration;

    /// <summary>
    /// Base <see cref="IJob"/> implementation.
    /// </summary>
    [DataContract(Namespace = Job.XmlNamespace)]
    public abstract class Job : IJob
    {
        #region Public Fields

        /// <summary>
        /// Gets the XML namespace used during job serialization.
        /// </summary>
        public const string XmlNamespace = "http://tastycodes.com/tasty-dll/job/";

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets the job's display name.
        /// </summary>
        [IgnoreDataMember]
        public abstract string Name { get; }

        /// <summary>
        /// Gets the number of times the job can be retried if it fails.
        /// When not overridden, defaults to 0 (no retries).
        /// </summary>
        [IgnoreDataMember]
        public int Retries
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets the timeout, in miliseconds, the job is allowed to run for.
        /// When not overridden, defaults to 60,000 (1 minute).
        /// </summary>
        [IgnoreDataMember]
        public virtual long Timeout 
        { 
            get { return 60000; }
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Creates a new job record representing an enqueue-able state for this instance.
        /// </summary>
        /// <returns>The created job record.</returns>
        public virtual JobRecord CreateRecord()
        {
            return this.CreateRecord(DateTime.UtcNow, null);
        }

        /// <summary>
        /// Creates a new job record representing an enqueue-able state for this instance.
        /// </summary>
        /// <param name="queueDate">The date to queue the job for execution on.</param>
        /// <param name="scheduleName">The name of the schedule to queue the job for, or null if not applicable.</param>
        /// <returns>The created job record.</returns>
        public virtual JobRecord CreateRecord(DateTime queueDate, string scheduleName)
        {
            Type type = GetType();

            return new JobRecord()
            {
                Data = this.Serialize(),
                JobType = String.Concat(type.FullName, ", ", type.Assembly.GetName().Name),
                Name = this.Name,
                QueueDate = queueDate,
                ScheduleName = scheduleName,
                Status = JobStatus.Queued
            };
        }

        /// <summary>
        /// Enqueues the job for execution.
        /// </summary>
        /// <returns>The job record that was persisted.</returns>
        public virtual JobRecord Enqueue()
        {
            return this.Enqueue(DateTime.UtcNow, null);
        }

        /// <summary>
        /// Enqueues the job for execution on a certin date and for a specific schedule.
        /// </summary>
        /// <param name="queueDate">The date to queue the job for execution on.</param>
        /// <param name="scheduleName">The name of the schedule to queue the job for, or null if not applicable.</param>
        /// <returns>The job record that was persisted.</returns>
        public virtual JobRecord Enqueue(DateTime queueDate, string scheduleName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes the job.
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Serializes the job state for enqueueing.
        /// </summary>
        /// <returns>The serialized job data.</returns>
        public virtual string Serialize()
        {
            DataContractSerializer serializer = new DataContractSerializer(this.GetType());
            StringBuilder sb = new StringBuilder();

            using (StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                using (XmlWriter xw = new XmlTextWriter(sw))
                {
                    serializer.WriteObject(xw, this);
                }
            }

            return sb.ToString();
        }

        #endregion
    }
}
