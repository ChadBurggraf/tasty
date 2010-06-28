using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Configuration;
using Tasty.Jobs;

namespace Tasty.Test
{
    [TestClass]
    public class JobRunnerTests
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            Bootstrapper.EnsureTestDatabase();
            Bootstrapper.CleanTestDatabaseJobs();
        }

        [TestMethod]
        [ExpectedException(typeof(SerializationException))]
        public void JobRunner_Serialize()
        {
            new JobRecord()
            {
                Data = Guid.NewGuid().ToString(),
                JobType = typeof(TestQuickJob)
            }.ToJob();
        }

        [TestMethod]
        public void JobRunner_IsRunning()
        {
            JobRunner.Instance.Start();
            Assert.IsTrue(JobRunner.Instance.IsRunning);
            JobRunner.Instance.Stop(false);
            Assert.IsFalse(JobRunner.Instance.IsRunning);
        }

        [TestMethod]
        public void JobRunner_CancelJobs()
        {
            JobRunner.Instance.Start();

            var id = new TestSlowJob().Enqueue().Id.Value;
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 2);

            var record = JobStore.Current.GetJob(id);
            Assert.AreEqual(JobStatus.Started, record.Status);

            record.Status = JobStatus.Canceling;
            JobStore.Current.UpdateJobs(new JobRecord[] { record }, null);
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 2);

            Assert.AreEqual(JobStatus.Canceled, JobStore.Current.GetJob(id).Status);
        }

        [TestMethod]
        public void JobRunner_DequeueJobs()
        {
            JobRunner.Instance.Start();

            var id = new TestSlowJob().Enqueue().Id.Value;
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 2);

            Assert.AreEqual(JobStatus.Started, JobStore.Current.GetJob(id).Status);
        }

        [TestMethod]
        public void JobRunner_FinishJobs()
        {
            JobRunner.Instance.Start();

            var id = new TestQuickJob().Enqueue().Id.Value;
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 3);

            Assert.AreEqual(JobStatus.Succeeded, JobStore.Current.GetJob(id).Status);
        }

        [TestMethod]
        public void JobRunner_TimeoutJobs()
        {
            JobRunner.Instance.Start();

            var id = new TestTimeoutJob().Enqueue().Id.Value;
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 3);

            Assert.AreEqual(JobStatus.TimedOut, JobStore.Current.GetJob(id).Status);
        }
    }
}
