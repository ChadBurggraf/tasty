//-----------------------------------------------------------------------
// <copyright file="ScheduledJob.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Tasty.Configuration;

    /// <summary>
    /// Base <see cref="IJob"/> implementation for scheduled jobs.
    /// </summary>
    [DataContract(Namespace = Job.XmlNamespace)]
    public abstract class ScheduledJob : Job
    {
        private NameValueCollection metadata;

        /// <summary>
        /// Gets the job's configured metadata.
        /// </summary>
        public NameValueCollection Metadata
        {
            get
            {
                return this.metadata ?? (this.metadata = new NameValueCollection());
            }
        }

        /// <summary>
        /// Creates a new scheduled job record for storing in the <see cref="IJobStore"/>.
        /// </summary>
        /// <param name="scheduleElement">The configured schedule element to create the record for.</param>
        /// <param name="jobElement">The configured job element to create the record for.</param>
        /// <param name="now">The queue and start date to create the record for.</param>
        /// <returns>A new scheduled job record.</returns>
        public static JobRecord CreateRecord(JobScheduleElement scheduleElement, JobScheduledJobElement jobElement, DateTime now)
        {
            if (scheduleElement == null)
            {
                throw new ArgumentNullException("scheduleElement", "scheduleElement cannot be null.");
            }

            if (String.IsNullOrEmpty(scheduleElement.Name))
            {
                throw new ArgumentException("scheduleElement cannot have an empty Name.", "scheduleElement");
            }

            if (jobElement == null)
            {
                throw new ArgumentNullException("jobElement", "jobElement cannot be null.");
            }

            if (String.IsNullOrEmpty(jobElement.JobType))
            {
                throw new ArgumentException("jobElement cannot have an empty JobType.", "jobElement");
            }

            if (now.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("now must be in UTC.", "now");
            }

            return new JobRecord()
            {
                Name = scheduleElement.Name,
                JobType = jobElement.JobType,
                QueueDate = now,
                ScheduleName = scheduleElement.Name,
                StartDate = now,
                Status = JobStatus.Started
            };
        }

        /// <summary>
        /// Creates a new <see cref="IJob"/> instance from the given scheduled job configuration.
        /// If the created object extends from <see cref="ScheduledJob"/>, its <see cref="ScheduledJob.Metadata"/>
        /// collection will be filled with any metadata defined in the configuration.
        /// </summary>
        /// <param name="element">The configuration to create the job from.</param>
        /// <returns>The created job.</returns>
        /// <exception cref="ConfigurationErrorsException">The configuration does not identify a valid type or the type identified does not implement <see cref="IJob"/>.</exception>
        public static IJob CreateFromConfiguration(JobScheduledJobElement element)
        {
            IJob job = null;
            Type concreteType = Type.GetType(element.JobType, false);

            if (concreteType != null && typeof(IJob).IsAssignableFrom(concreteType))
            {
                try
                {
                    job = (IJob)Activator.CreateInstance(concreteType);
                    ScheduledJob sj = job as ScheduledJob;

                    if (sj != null)
                    {
                        sj.Metadata.FillWith(element.Metadata);
                    }
                }
                catch (ArgumentException)
                {
                }
                catch (NotSupportedException)
                {
                }
                catch (TargetInvocationException)
                {
                }
                catch (MethodAccessException)
                {
                }
                catch (MemberAccessException)
                {
                }
                catch (TypeLoadException)
                {
                }
            }

            if (job == null)
            {
                throw new ConfigurationErrorsException(String.Format(CultureInfo.InvariantCulture, "The type \"{0}\" could not be instantiated into an object implementing the Tasty.Jobs.IJob interface.", element.JobType), element.ElementInformation.Source, element.ElementInformation.LineNumber);
            }

            return job;
        }

        /// <summary>
        /// Gets the next execute date for the scheduled job identified by the given configuration element.
        /// </summary>
        /// <param name="element">The configuration element of the scheduled job to get the next execute date for.</param>
        /// <param name="lastExecuteDate">The schedule's last execution date, if applicable (in UTC).</param>
        /// <param name="now">The current date (in UTC).</param>
        /// <returns>The schedule's next execute date.</returns>
        public static DateTime GetNextExecuteDate(JobScheduleElement element, DateTime? lastExecuteDate, DateTime now)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element", "element cannot be null");
            }

            if (element.RepeatHours <= 0)
            {
                throw new ConfigurationErrorsException("A job schedule's repeatHours must be greater than 0.", element.ElementInformation.Source, element.ElementInformation.LineNumber);
            }

            if (lastExecuteDate != null && lastExecuteDate.Value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("lastExecuteDate must be in UTC.", "lastExecuteDate");
            }

            if (now.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("now must be in UTC.", "now");
            }

            if (lastExecuteDate == null)
            {
                lastExecuteDate = element.StartOn.ToUniversalTime();
            }

            if (now < lastExecuteDate.Value)
            {
                return lastExecuteDate.Value;
            }

            double hours = now.Subtract(lastExecuteDate.Value).TotalHours;
            int repeats = (int)(hours / element.RepeatHours);

            DateTime date = lastExecuteDate.Value.AddHours(repeats * element.RepeatHours);

            if (date == lastExecuteDate.Value)
            {
                date = date.AddHours(element.RepeatHours);
            }

            return date;
        }
    }
}