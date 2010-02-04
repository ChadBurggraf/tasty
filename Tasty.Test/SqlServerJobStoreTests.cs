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

        protected JobRecord Enqueue(IJob job)
        {
            return Store.CreateJob(new JobRecord()
            {
                Data = job.Serialize(),
                JobType = job.GetType(),
                Name = job.Name,
                QueueDate = DateTime.UtcNow,
                Status = JobStatus.Queued
            });
        }

        [TestMethod]
        public void SqlServerJobStore_CancellingJobs()
        {
            var record1 = Enqueue(new SqlServerTestJob());
            var record2 = Enqueue(new SqlServerTestJob());
            var record3 = Enqueue(new SqlServerTestJob());

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
            var job1 = new SqlServerTestJob();

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
            var record1 = Enqueue(new SqlServerTestJob());
            var record2 = Enqueue(new SqlServerTestJob());
            var record3 = Enqueue(new SqlServerTestJob());

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
            Assert.IsTrue(0 < Enqueue(new SqlServerTestJob()).Id);
        }

        [TestMethod]
        public void SqlServerJobStore_FinishingJobs()
        {
            var record1 = Enqueue(new SqlServerTestJob());
            var record2 = Enqueue(new SqlServerTestJob());
            var record3 = Enqueue(new SqlServerTestJob());

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
            var id = Enqueue(new SqlServerTestJob()).Id.Value;
            Assert.IsNotNull(Store.GetJob(id));
        }

        [TestMethod]
        public void SqlServerJobStore_TimingOutJobs()
        {
            var record1 = Enqueue(new SqlServerTestJob());
            var record2 = Enqueue(new SqlServerTestJob());
            var record3 = Enqueue(new SqlServerTestJob());

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
            var origJob = new SqlServerTestJob();
            var record = Enqueue(origJob);

            var newJob = new SqlServerTestJob();
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

    [DataContract(Namespace = Job.XmlNamespace)]
    internal class SqlServerTestJob : Job
    {
        public SqlServerTestJob()
        {
            Id = Guid.NewGuid();
        }

        [DataMember]
        public Guid Id { get; set; }

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
