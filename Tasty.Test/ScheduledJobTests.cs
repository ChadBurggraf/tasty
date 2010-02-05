using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Configuration;
using Tasty.Jobs;

namespace Tasty.Test
{
    [TestClass]
    public class ScheduledJobTests
    {
        [TestMethod]
        public void ScheduledJobs_CreateFromConfiguration()
        {
            var config1 = new JobScheduledJobElement() { JobType = typeof(TestIdJob).AssemblyQualifiedName };
            var config2 = new JobScheduledJobElement() { JobType = typeof(TestScheduledJob).AssemblyQualifiedName };
            config2.Metadata.Add("key1", "value1");
            config2.Metadata.Add("key2", "value2");

            Assert.IsNotNull(ScheduledJob.CreateFromConfiguration(config1));

            var job2 = ScheduledJob.CreateFromConfiguration(config2);
            Assert.IsNotNull(job2);
            Assert.AreEqual("value1", ((ScheduledJob)job2).Metadata["key1"]);
            Assert.AreEqual("value2", ((ScheduledJob)job2).Metadata["key2"]);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void ScheduledJobs_FailCreateFromConfiguration()
        {
            ScheduledJob.CreateFromConfiguration(new JobScheduledJobElement() { JobType = "not a job type" });
        }

        [TestMethod]
        public void ScheduledJobs_GetNextExecuteDate()
        {
            //
            // This all needs to be more thorough, but I'm feeling lazy and I'm pretty confident it works.
            //

            DateTime now = DateTime.Now;
            DateTime nowPlusOneHour = now.AddHours(1);
            DateTime nowPlusOneWeek = now.AddDays(7);
            DateTime nowMinusOneWeek = now.AddDays(-7);
            DateTime nowMinusOneDay = now.AddDays(-1);

            Assert.AreEqual(nowPlusOneHour, ScheduledJob.GetNextExecuteDate(new JobScheduleElement()
            {
                StartOn = nowPlusOneHour,
                Repeat = JobScheduleRepeatType.Daily
            }, now));

            Assert.AreEqual(nowPlusOneWeek, ScheduledJob.GetNextExecuteDate(new JobScheduleElement()
            {
                StartOn = nowPlusOneWeek,
                Repeat = JobScheduleRepeatType.Weekly
            }, now));

            Assert.AreEqual(now, ScheduledJob.GetNextExecuteDate(new JobScheduleElement()
            {
                StartOn = now,
                Repeat = JobScheduleRepeatType.Daily
            }, now));

            Assert.AreEqual(now, ScheduledJob.GetNextExecuteDate(new JobScheduleElement()
            {
                StartOn = now,
                Repeat = JobScheduleRepeatType.Hourly
            }, now));

            Assert.AreEqual(now, ScheduledJob.GetNextExecuteDate(new JobScheduleElement()
            {
                StartOn = now,
                Repeat = JobScheduleRepeatType.Weekly
            }, now));

            Assert.AreEqual(now, ScheduledJob.GetNextExecuteDate(new JobScheduleElement()
            {
                StartOn = nowMinusOneDay,
                Repeat = JobScheduleRepeatType.Daily
            }, now));
        }
    }
}
