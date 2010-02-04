using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Configuration;
using Tasty.Jobs;

namespace Tasty.Test
{
    [TestClass]
    public class SqlServerJobStoreTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Bootstrapper.CreateTestDatabase();
        }

        [TestMethod]
        public void SqlServerJobStore_CancellingJobs()
        {
            var record1 = new TestJob().Enqueue();
            var record2 = new TestJob().Enqueue();
            var record3 = new TestJob().Enqueue();

            record1.Status = JobStatus.Canceling;
            Job.ConfiguredStore.UpdateJobs(new JobRecord[] { record1 }, null);

            Job.ConfiguredStore.CancelingJobs(delegate(IEnumerable<JobRecord> records)
            {
                Assert.AreEqual(1, records.Where(r => r.Id == record1.Id).Count());
                Assert.AreEqual(0, records.Where(r => r.Id == record2.Id).Count());
                Assert.AreEqual(0, records.Where(r => r.Id == record3.Id).Count());
            });
        }

        [TestMethod]
        public void SqlServerJobStore_CreateJob()
        {
            var job1 = new TestJob();

            var record = new JobRecord()
            {
                Data = job1.Serialize(),
                JobType = job1.GetType(),
                Name = job1.Name,
                QueueDate = DateTime.UtcNow,
                Status = JobStatus.Queued
            };

            Assert.IsTrue(0 < Job.ConfiguredStore.CreateJob(record).Id);
        }

        [TestMethod]
        public void SqlServerJobStore_DequeueJobs()
        {
            var record1 = new TestJob().Enqueue();
            var record2 = new TestJob().Enqueue();
            var record3 = new TestJob().Enqueue();

            record1.Status = JobStatus.Succeeded;
            record1.FinishDate = record1.StartDate = DateTime.UtcNow;
            Job.ConfiguredStore.UpdateJobs(new JobRecord[] { record1 }, null);

            Job.ConfiguredStore.DequeueJobs(delegate(IEnumerable<JobRecord> records)
            {
                Assert.AreEqual(0, records.Where(r => r.Id == record1.Id).Count());
                Assert.AreEqual(1, records.Where(r => r.Id == record2.Id).Count());
                Assert.AreEqual(1, records.Where(r => r.Id == record2.Id).Count());
            }, 
            TastySettings.Section.Jobs.MaximumConcurrency);
        }

        [TestMethod]
        public void SqlServerJobStore_EnqueueJob()
        {
            Assert.IsTrue(0 < new TestJob().Enqueue().Id);
        }

        [TestMethod]
        public void SqlServerJobStore_UpdateJob()
        {
            var record = new TestJob().Enqueue();
            
            record.Data = Guid.NewGuid().ToString();
            record.Exception = Guid.NewGuid().ToString();
            record.FinishDate = DateTime.UtcNow.AddDays(-1);
            record.Name = Guid.NewGuid().ToString();
            record.QueueDate = DateTime.UtcNow.AddDays(-1);
            record.ScheduleName = Guid.NewGuid().ToString();
            record.StartDate = DateTime.UtcNow.AddDays(-1);

            Job.ConfiguredStore.UpdateJobs(new JobRecord[] { record }, null);
            Job.ConfiguredStore.DequeueJobs(delegate(IEnumerable<JobRecord> records)
            {
                var updated = records.Where(r => r.Id == record.Id).FirstOrDefault();

                Assert.AreEqual(record.Data, updated.Data);
                Assert.AreEqual(record.Exception, updated.Exception);
                SqlDateTimeAssert.AreEqual(record.FinishDate, updated.FinishDate);
                Assert.AreEqual(record.Name, updated.Name);
                SqlDateTimeAssert.AreEqual(record.QueueDate, updated.QueueDate);
                Assert.AreEqual(record.ScheduleName, updated.ScheduleName);
                SqlDateTimeAssert.AreEqual(record.StartDate, updated.StartDate);
            },
            TastySettings.Section.Jobs.MaximumConcurrency);
            
        }
    }

    public class TestJob : Job
    {
        public override string Name
        {
            get { return "Tasty Test Job"; }
        }

        public override void Execute()
        {
        }
    }
}
