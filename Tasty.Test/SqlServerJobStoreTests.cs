using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Configuration;
using Tasty.Jobs;

namespace Tasty.Test
{
    [TestClass]
    public class SqlServerJobStoreTests
    {
        public SqlServerJobStoreTests()
        {
            Store = new SqlServerJobStore();
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            Bootstrapper.EnsureTestDatabase();
        }

        protected IJobStore Store { get; private set; }

        protected JobRecord CreateJob(IJob job, DateTime queueDate, JobStatus status, string scheduleName)
        {
            return Store.CreateJob(new JobRecord()
            {
                Data = job.Serialize(),
                JobType = job.GetType(),
                Name = job.Name,
                QueueDate = queueDate,
                Status = status,
                ScheduleName = scheduleName
            });
        }

        protected JobRecord CreateQueued(IJob job)
        {
            return CreateJob(job, DateTime.UtcNow, JobStatus.Queued, null);
        }

        protected JobRecord CreateSucceeded(IJob job, DateTime queueDate, string scheduleName)
        {
            return CreateJob(job, queueDate, JobStatus.Succeeded, scheduleName);
        }

        [TestMethod]
        public void SqlServerJobStore_CancellingJobs()
        {
            var record1 = CreateQueued(new TestIdJob());
            var record2 = CreateQueued(new TestIdJob());
            var record3 = CreateQueued(new TestIdJob());

            record1.Status = JobStatus.Canceling;
            Store.UpdateJobs(new JobRecord[] { record1 }, null);

            Store.CancelingJobs(new int[] { record1.Id.Value, record2.Id.Value, record3.Id.Value }, delegate(IEnumerable<JobRecord> records)
            {
                Assert.AreEqual(1, records.Where(r => r.Id == record1.Id).Count());
                Assert.AreEqual(0, records.Where(r => r.Id == record2.Id).Count());
                Assert.AreEqual(0, records.Where(r => r.Id == record3.Id).Count());
            });
        }

        [TestMethod]
        public void SqlServerJobStore_CreateJob()
        {
            var job1 = new TestIdJob();

            var record = new JobRecord()
            {
                Data = job1.Serialize(),
                JobType = job1.GetType(),
                Name = job1.Name,
                QueueDate = DateTime.UtcNow,
                Status = JobStatus.Queued
            };

            Assert.IsTrue(0 < Store.CreateJob(record).Id);
        }

        [TestMethod]
        public void SqlServerJobStore_DequeueingJobs()
        {
            var record1 = CreateQueued(new TestIdJob());
            var record2 = CreateQueued(new TestIdJob());
            var record3 = CreateQueued(new TestIdJob());

            record1.Status = JobStatus.Succeeded;
            record1.FinishDate = record1.StartDate = DateTime.UtcNow;
            Store.UpdateJobs(new JobRecord[] { record1 }, null);

            Store.DequeueingJobs(100, delegate(IEnumerable<JobRecord> records)
            {
                Assert.AreEqual(0, records.Where(r => r.Id == record1.Id).Count());
                Assert.AreEqual(1, records.Where(r => r.Id == record2.Id).Count());
                Assert.AreEqual(1, records.Where(r => r.Id == record2.Id).Count());
            });
        }

        [TestMethod]
        public void SqlServerJobStore_EnqueueJob()
        {
            Assert.IsTrue(0 < CreateQueued(new TestIdJob()).Id);
        }

        [TestMethod]
        public void SqlServerJobStore_FinishingJobs()
        {
            var record1 = CreateQueued(new TestIdJob());
            var record2 = CreateQueued(new TestIdJob());
            var record3 = CreateQueued(new TestIdJob());

            record1.Status = JobStatus.Canceling;
            Store.UpdateJobs(new JobRecord[] { record1 }, null);

            record2.Status = JobStatus.Started;
            Store.UpdateJobs(new JobRecord[] { record2 }, null);

            record3.Status = JobStatus.Started;
            Store.UpdateJobs(new JobRecord[] { record3 }, null);

            Store.FinishingJobs(new int[] { record1.Id.Value, record2.Id.Value, record3.Id.Value }, delegate(IEnumerable<JobRecord> records)
            {
                Assert.AreEqual(0, records.Where(r => r.Id == record1.Id).Count());
                Assert.AreEqual(1, records.Where(r => r.Id == record2.Id).Count());
                Assert.AreEqual(1, records.Where(r => r.Id == record3.Id).Count());
            });
        }

        [TestMethod]
        public void SqlServerJobStore_GetJob()
        {
            var id = CreateQueued(new TestIdJob()).Id.Value;
            Assert.IsNotNull(Store.GetJob(id));
        }

        [TestMethod]
        public void SqlServerJobStore_GetLatestScheduledJobs()
        {
            JobRunner.Instance.Stop();

            var schedules = new JobScheduleElementCollection();
            var sA = new JobScheduleElement() { Name = "A" };
            schedules.Add(sA);
            var sB = new JobScheduleElement() { Name = "B" };
            schedules.Add(sB);

            sA.ScheduledJobs.Add(new JobScheduledJobElement() { JobType = typeof(TestIdJob).AssemblyQualifiedName });
            sA.ScheduledJobs.Add(new JobScheduledJobElement() { JobType = typeof(TestSlowJob).AssemblyQualifiedName });
            sB.ScheduledJobs.Add(new JobScheduledJobElement() { JobType = typeof(TestSlowJob).AssemblyQualifiedName });
            sB.ScheduledJobs.Add(new JobScheduledJobElement() { JobType = typeof(TestTimeoutJob).AssemblyQualifiedName });

            var idA1 = CreateSucceeded(new TestIdJob(), DateTime.Parse("2/1/10"), "A").Id.Value;
            var idA2 = CreateSucceeded(new TestIdJob(), DateTime.Parse("2/2/10"), "A").Id.Value;
            var idA3 = CreateSucceeded(new TestIdJob(), DateTime.Parse("2/3/10"), "A").Id.Value;

            var slowA1 = CreateSucceeded(new TestSlowJob(), DateTime.Parse("2/2/10"), "A").Id.Value;
            var slowA2 = CreateSucceeded(new TestSlowJob(), DateTime.Parse("2/3/10"), "A").Id.Value;

            var slowB1 = CreateSucceeded(new TestSlowJob(), DateTime.Parse("2/2/10"), "B").Id.Value;
            var slowB2 = CreateSucceeded(new TestSlowJob(), DateTime.Parse("2/3/10"), "B").Id.Value;

            var timeoutC1 = CreateSucceeded(new TestTimeoutJob(), DateTime.Parse("2/3/10"), "C").Id.Value;

            var latest = Store.GetLatestScheduledJobs(schedules);

            Assert.AreEqual(3, latest.Count());
            Assert.AreEqual(1, latest.Where(r => r.Record.Id == idA3).Count());
            Assert.AreEqual(1, latest.Where(r => r.Record.Id == slowA2).Count());
            Assert.AreEqual(1, latest.Where(r => r.Record.Id == slowB2).Count());
        }

        [TestMethod]
        public void SqlServerJobStore_TimingOutJobs()
        {
            var record1 = CreateQueued(new TestIdJob());
            var record2 = CreateQueued(new TestIdJob());
            var record3 = CreateQueued(new TestIdJob());

            record1.Status = JobStatus.Canceling;
            Store.UpdateJobs(new JobRecord[] { record1 }, null);

            record2.Status = JobStatus.Started;
            Store.UpdateJobs(new JobRecord[] { record2 }, null);

            record3.Status = JobStatus.Started;
            Store.UpdateJobs(new JobRecord[] { record3 }, null);

            Store.TimingOutJobs(new int[] { record1.Id.Value, record2.Id.Value, record3.Id.Value }, delegate(IEnumerable<JobRecord> records)
            {
                Assert.AreEqual(0, records.Where(r => r.Id == record1.Id).Count());
                Assert.AreEqual(1, records.Where(r => r.Id == record2.Id).Count());
                Assert.AreEqual(1, records.Where(r => r.Id == record3.Id).Count());
            });
        }

        [TestMethod]
        public void SqlServerJobStore_UpdateJob()
        {
            var origJob = new TestIdJob();
            var record = CreateQueued(origJob);

            var newJob = new TestIdJob();
            record.Data = newJob.Serialize();
            record.Exception = new ExceptionXElement(new Exception()).ToString();
            record.FinishDate = DateTime.UtcNow.AddDays(-1);
            record.Name = Guid.NewGuid().ToString();
            record.QueueDate = DateTime.UtcNow.AddDays(-1);
            record.ScheduleName = Guid.NewGuid().ToString();
            record.StartDate = DateTime.UtcNow.AddDays(-1);

            Store.UpdateJobs(new JobRecord[] { record }, null);
            Store.DequeueingJobs(100, delegate(IEnumerable<JobRecord> records)
            {
                var updated = records.Where(r => r.Id == record.Id).FirstOrDefault();

                Assert.AreEqual(record.Data, updated.Data);
                Assert.IsFalse(String.IsNullOrEmpty(updated.Exception));
                SqlDateTimeAssert.AreEqual(record.FinishDate, updated.FinishDate);
                Assert.AreEqual(record.Name, updated.Name);
                SqlDateTimeAssert.AreEqual(record.QueueDate, updated.QueueDate);
                Assert.AreEqual(record.ScheduleName, updated.ScheduleName);
                SqlDateTimeAssert.AreEqual(record.StartDate, updated.StartDate);
            });
        }
    }
}
