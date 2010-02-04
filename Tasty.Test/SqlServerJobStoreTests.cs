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
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            Bootstrapper.CreateTestDatabase();
        }

        [TestMethod]
        public void SqlServerJobStore_CancellingJobs()
        {
            var record1 = new SqlServerTestJob().Enqueue();
            var record2 = new SqlServerTestJob().Enqueue();
            var record3 = new SqlServerTestJob().Enqueue();

            record1.Status = JobStatus.Canceling;
            JobStore.Current.UpdateJobs(new JobRecord[] { record1 }, null);

            JobStore.Current.CancelingJobs(new int[] { record1.Id.Value, record2.Id.Value, record3.Id.Value }, delegate(IEnumerable<JobRecord> records)
            {
                Assert.AreEqual(1, records.Where(r => r.Id == record1.Id).Count());
                Assert.AreEqual(0, records.Where(r => r.Id == record2.Id).Count());
                Assert.AreEqual(0, records.Where(r => r.Id == record3.Id).Count());
            });
        }

        [TestMethod]
        public void SqlServerJobStore_CreateJob()
        {
            var job1 = new SqlServerTestJob();

            var record = new JobRecord()
            {
                Data = job1.Serialize(),
                JobType = job1.GetType(),
                Name = job1.Name,
                QueueDate = DateTime.UtcNow,
                Status = JobStatus.Queued
            };

            Assert.IsTrue(0 < JobStore.Current.CreateJob(record).Id);
        }

        [TestMethod]
        public void SqlServerJobStore_DequeueingJobs()
        {
            var record1 = new SqlServerTestJob().Enqueue();
            var record2 = new SqlServerTestJob().Enqueue();
            var record3 = new SqlServerTestJob().Enqueue();

            record1.Status = JobStatus.Succeeded;
            record1.FinishDate = record1.StartDate = DateTime.UtcNow;
            JobStore.Current.UpdateJobs(new JobRecord[] { record1 }, null);

            JobStore.Current.DequeueingJobs(100, delegate(IEnumerable<JobRecord> records)
            {
                Assert.AreEqual(0, records.Where(r => r.Id == record1.Id).Count());
                Assert.AreEqual(1, records.Where(r => r.Id == record2.Id).Count());
                Assert.AreEqual(1, records.Where(r => r.Id == record2.Id).Count());
            });
        }

        [TestMethod]
        public void SqlServerJobStore_EnqueueJob()
        {
            Assert.IsTrue(0 < new SqlServerTestJob().Enqueue().Id);
        }

        [TestMethod]
        public void SqlServerJobStore_FinishingJobs()
        {
            var record1 = new SqlServerTestJob().Enqueue();
            var record2 = new SqlServerTestJob().Enqueue();
            var record3 = new SqlServerTestJob().Enqueue();

            record1.Status = JobStatus.Canceling;
            JobStore.Current.UpdateJobs(new JobRecord[] { record1 }, null);

            record2.Status = JobStatus.Started;
            JobStore.Current.UpdateJobs(new JobRecord[] { record2 }, null);

            record3.Status = JobStatus.Started;
            JobStore.Current.UpdateJobs(new JobRecord[] { record3 }, null);

            JobStore.Current.FinishingJobs(new int[] { record1.Id.Value, record2.Id.Value, record3.Id.Value }, delegate(IEnumerable<JobRecord> records)
            {
                Assert.AreEqual(0, records.Where(r => r.Id == record1.Id).Count());
                Assert.AreEqual(1, records.Where(r => r.Id == record2.Id).Count());
                Assert.AreEqual(1, records.Where(r => r.Id == record3.Id).Count());
            });
        }

        [TestMethod]
        public void SqlServerJobStore_TimingOutJobs()
        {
            var record1 = new SqlServerTestJob().Enqueue();
            var record2 = new SqlServerTestJob().Enqueue();
            var record3 = new SqlServerTestJob().Enqueue();

            record1.Status = JobStatus.Canceling;
            JobStore.Current.UpdateJobs(new JobRecord[] { record1 }, null);

            record2.Status = JobStatus.Started;
            JobStore.Current.UpdateJobs(new JobRecord[] { record2 }, null);

            record3.Status = JobStatus.Started;
            JobStore.Current.UpdateJobs(new JobRecord[] { record3 }, null);

            JobStore.Current.TimingOutJobs(new int[] { record1.Id.Value, record2.Id.Value, record3.Id.Value }, delegate(IEnumerable<JobRecord> records)
            {
                Assert.AreEqual(0, records.Where(r => r.Id == record1.Id).Count());
                Assert.AreEqual(1, records.Where(r => r.Id == record2.Id).Count());
                Assert.AreEqual(1, records.Where(r => r.Id == record3.Id).Count());
            });
        }

        [TestMethod]
        public void SqlServerJobStore_UpdateJob()
        {
            var record = new SqlServerTestJob().Enqueue();
            
            record.Data = Guid.NewGuid().ToString();
            record.Exception = Guid.NewGuid().ToString();
            record.FinishDate = DateTime.UtcNow.AddDays(-1);
            record.Name = Guid.NewGuid().ToString();
            record.QueueDate = DateTime.UtcNow.AddDays(-1);
            record.ScheduleName = Guid.NewGuid().ToString();
            record.StartDate = DateTime.UtcNow.AddDays(-1);

            JobStore.Current.UpdateJobs(new JobRecord[] { record }, null);
            JobStore.Current.DequeueingJobs(100, delegate(IEnumerable<JobRecord> records)
            {
                var updated = records.Where(r => r.Id == record.Id).FirstOrDefault();

                Assert.AreEqual(record.Data, updated.Data);
                Assert.AreEqual(record.Exception, updated.Exception);
                SqlDateTimeAssert.AreEqual(record.FinishDate, updated.FinishDate);
                Assert.AreEqual(record.Name, updated.Name);
                SqlDateTimeAssert.AreEqual(record.QueueDate, updated.QueueDate);
                Assert.AreEqual(record.ScheduleName, updated.ScheduleName);
                SqlDateTimeAssert.AreEqual(record.StartDate, updated.StartDate);
            });
        }
    }

    public class SqlServerTestJob : Job
    {
        public override string Name
        {
            get { return "Tasty Test Job"; }
        }

        public override long Timeout
        {
            get
            {
                return 10; // 10 ms.
            }
        }

        public override void Execute()
        {
        }
    }
}
