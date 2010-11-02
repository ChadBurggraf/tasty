using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
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
        public void ScheduledJobs_ShouldExecute()
        {
            JobScheduleElement element = new JobScheduleElement() { StartOn = DateTime.UtcNow.AddHours(-48), RepeatHours = 1 };
            Assert.IsTrue(ScheduledJob.ShouldExecute(element, 500, DateTime.UtcNow));
            Thread.Sleep(600);
            Assert.IsFalse(ScheduledJob.ShouldExecute(element, 500, DateTime.UtcNow));

            element.StartOn = DateTime.UtcNow.AddYears(-5);
            element.RepeatHours = .0166666667; // This kinda sucks - 10 significant digits to have accuracy < 500ms over 5 years.
            Assert.IsTrue(ScheduledJob.ShouldExecute(element, 500, DateTime.UtcNow));
            Thread.Sleep(600);
            Assert.IsTrue(ScheduledJob.ShouldExecute(element, 500, DateTime.UtcNow));
        }
    }
}
