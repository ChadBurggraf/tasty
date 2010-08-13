﻿using System;
using System.Collections.Generic;
using System.Configuration;
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
        private IJobStore jobStore;
        private JobRunner jobRunner;

        public JobRunnerTests()
        {
            TastySettings.Section.Jobs.Heartbeat = 1000;

            this.jobStore = new MemoryJobStore();
            this.jobRunner = JobRunner.GetInstance(this.jobStore);
            this.jobRunner.Error += new EventHandler<JobErrorEventArgs>(JobRunnerError);
            this.jobRunner.Start();
        }

        [TestMethod]
        public void JobRunner_CancelJobs()
        {
            this.jobRunner.Start();

            var id = new TestSlowJob().Enqueue(this.jobStore).Id.Value;
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 2);

            var record = this.jobStore.GetJob(id);
            Assert.AreEqual(JobStatus.Started, record.Status);

            record.Status = JobStatus.Canceling;
            this.jobStore.SaveJob(record);
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 2);

            Assert.AreEqual(JobStatus.Canceled, this.jobStore.GetJob(id).Status);
        }

        [TestMethod]
        public void JobRunner_DequeueJobs()
        {
            this.jobRunner.Start();

            var id = new TestSlowJob().Enqueue(this.jobStore).Id.Value;
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 2);

            Assert.AreEqual(JobStatus.Started, this.jobStore.GetJob(id).Status);
        }

        public void JobRunner_ExecuteScheduledJobs()
        {
            this.jobRunner.Start();

            JobScheduleElementCollection schedules = new JobScheduleElementCollection();

            JobScheduleElement sched1 = new JobScheduleElement() 
            {
                RepeatHours = 24,
                StartOn = DateTime.UtcNow.AddYears(-1)
            };

            schedules.Add(sched1);

            JobScheduleElement sched2 = new JobScheduleElement()
            {
                RepeatHours = .5,
                StartOn = DateTime.UtcNow.AddDays(-1)
            };

            schedules.Add(sched2);

            JobScheduleElement sched3 = new JobScheduleElement()
            {
                RepeatHours = .1,
                StartOn = DateTime.UtcNow.AddDays(1)
            };

            schedules.Add(sched2);

            JobScheduledJobElement job1 = new JobScheduledJobElement()
            {
                JobType = JobRecord.JobTypeString(typeof(TestIdJob))
            };

            JobScheduledJobElement job2 = new JobScheduledJobElement()
            {
                JobType = JobRecord.JobTypeString(typeof(TestScheduledJob))
            };

            sched1.ScheduledJobs.Add(job1);
            sched1.ScheduledJobs.Add(job2);
            sched2.ScheduledJobs.Add(job2);

            this.jobRunner.Schedules = schedules;
        }

        [TestMethod]
        public void JobRunner_FinishJobs()
        {
            this.jobRunner.Start();

            var id = new TestQuickJob().Enqueue(this.jobStore).Id.Value;
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 2);

            Assert.AreEqual(JobStatus.Succeeded, this.jobStore.GetJob(id).Status);
        }

        [TestMethod]
        public void JobRunner_TimeoutJobs()
        {
            this.jobRunner.Start();

            var id = new TestTimeoutJob().Enqueue(this.jobStore).Id.Value;
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 2);

            Assert.AreEqual(JobStatus.TimedOut, this.jobStore.GetJob(id).Status);
        }

        private void JobRunnerError(object sender, JobErrorEventArgs e)
        {
            if (e.Exception != null)
            {
                throw e.Exception;
            }

            throw new Exception(Environment.StackTrace);
        }
    }
}
