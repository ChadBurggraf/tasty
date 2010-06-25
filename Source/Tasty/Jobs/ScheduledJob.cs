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
        /// Gets the next execution date for the given schedule and the given value of "now".
        /// </summary>
        /// <param name="element">The configured job schedule to get the next execution date for.</param>
        /// <param name="now">The reference date to compare schedule dates to.</param>
        /// <returns>The schedule's next execution date.</returns>
        public static DateTime GetNextExecuteDate(JobScheduleElement element, DateTime now)
        {
            if (now.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("now must be in UTC.", "now");
            }

            DateTime startOn = element.StartOn.ToUniversalTime();

            if (now < startOn)
            {
                return startOn;
            }

            switch (element.Repeat)
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