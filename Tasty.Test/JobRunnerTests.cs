using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        [TestMethod]
        [ExpectedException(typeof(SerializationException))]
        public void JobRunner_Serialize()
        {
            new JobRecord()
            {
                Data = Guid.NewGuid().ToString(),
                JobType = typeof(JobRunnerQuickJob)
            }.ToJob();
        }

        [TestMethod]
        public void JobRunner_IsRunning()
        {
            JobRunner.Instance.Start();
            Assert.IsTrue(JobRunner.Instance.IsRunning);
            JobRunner.Instance.Stop();
            Assert.IsFalse(JobRunner.Instance.IsRunning);
        }

        [TestMethod]
        public void JobRunner_CancelJobs()
        {
            JobRunner.Instance.Start();

            var id = new JobRunnerSlowJob().Enqueue().Id.Value;
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 2);

            var record = JobStore.Current.GetJob(id);
            record.Status = JobStatus.Canceling;
            JobStore.Current.UpdateJobs(new JobRecord[] { record }, null);
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat);

            Assert.AreEqual(JobStatus.Canceled, JobStore.Current.GetJob(id).Status);
        }

        [TestMethod]
        public void JobRunner_DequeueJobs()
        {
            JobRunner.Instance.Start();

            var id = new JobRunnerSlowJob().Enqueue().Id.Value;
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 2);

            Assert.AreEqual(JobStatus.Started, JobStore.Current.GetJob(id).Status);
        }

        [TestMethod]
        public void JobRunner_FinishJobs()
        {
            JobRunner.Instance.Start();

            var id = new JobRunnerQuickJob().Enqueue().Id.Value;
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 3);

            Assert.AreEqual(JobStatus.Succeeded, JobStore.Current.GetJob(id).Status);
        }

        [TestMethod]
        public void JobRunner_TimeoutJobs()
        {
            JobRunner.Instance.Start();

            var id = new JobRunnerTimeoutJob().Enqueue().Id.Value;
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 3);

            Assert.AreEqual(JobStatus.TimedOut, JobStore.Current.GetJob(id).Status);
        }
    }

    [DataContract(Namespace = Job.XmlNamespace)]
    internal class JobRunnerQuickJob : Job
    {
        public override string Name
        {
            get { return "Job Runner Quick Job"; }
        }

        public override void Execute()
        {
        }
    }

    [DataContract(Namespace = Job.XmlNamespace)]
    internal class JobRunnerSlowJob : Job
    {
        public override string Name
        {
            get { return "Job Runner Slow Job"; }
        }

        public override void Execute()
        {
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 10);
        }
    }

    [DataContract(Namespace = Job.XmlNamespace)]
    internal class JobRunnerTimeoutJob : Job
    {
        public override string Name
        {
            get { return "Job Runner Timeout Job"; }
        }

        public override long Timeout
        {
            get
            {
                return TastySettings.Section.Jobs.Heartbeat;
            }
        }

        public override void Execute()
        {
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 10);
        }
    }
}
