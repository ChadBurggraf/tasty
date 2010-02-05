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
    using Tasty.Configuration;

    /// <summary>
    /// Base <see cref="IJob"/> implementation for scheduled jobs.
    /// </summary>
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
        /// Creates a new <see cref="IJob"/> instance from the given scheduled job configuration.
        /// If the created object extends from <see cref="ScheduledJob"/>, its <see cref="ScheduledJob.Metadata"/>
        /// collection will be filled with any metadata defined in the configuration.
        /// </summary>
        /// <param name="config">The configuration to create the job from.</param>
        /// <returns>The created job.</returns>
        /// <exception cref="ConfigurationErrorsException">The configuration does not identiy a valid type or the type identified does not implement <see cref="IJob"/>.</exception>
        public static IJob CreateFromConfiguration(JobScheduledJobElement config)
        {
            IJob job = null;
            Type concreteType = Type.GetType(config.JobType, false);

            if (concreteType != null && typeof(IJob).IsAssignableFrom(concreteType))
            {
                try
                {
                    job = (IJob)Activator.CreateInstance(concreteType);
                    ScheduledJob sj = job as ScheduledJob;

                    if (sj != null)
                    {
                        sj.Metadata.FillWith(config.Metadata);
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
                throw new ConfigurationErrorsException(String.Format(CultureInfo.InvariantCulture, "The type \"{0}\" could not be instantiated into an object implementing the Tasty.Jobs.IJob interface.", config.JobType));
            }

            return job;
        }

        /// <summary>
        /// Gets the next execution date for the given schedule and the given value of "now".
        /// </summary>
        /// <param name="config">The configured job schedule to get the next execution date for.</param>
        /// <param name="now">The reference date to compare schedule dates to.</param>
        /// <returns>The schedule's next execution date.</returns>
        public static DateTime GetNextExecuteDate(JobScheduleElement config, DateTime now)
        {
            DateTime startOn = config.StartOn.ToUniversalTime();

            if (now < startOn)
            {
                return startOn;
            }

            switch (config.Repeat)
            {
                case JobScheduleRepeatType.Daily:
                    int days = (int)Math.Ceiling(now.Subtract(startOn).TotalDays);
                    return startOn.AddDays(days);
                case JobScheduleRepeatType.Hourly:
                    int hours = (int)Math.Ceiling(now.Subtract(startOn).TotalHours);
                    return startOn.AddHours(hours);
                case JobScheduleRepeatType.Weekly:
                    int weekDays = (int)Math.Ceiling(now.Subtract(startOn).TotalDays);
                    return startOn.AddDays((int)Math.Ceiling((double)weekDays / 7));
                default:
                    throw new NotImplementedException();
            }
        }
    }
}