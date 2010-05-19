using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Configuration;
using Tasty.Jobs;

namespace Tasty.Test
{
    public abstract class JobStoreTests
    {
        protected JobStoreTests(IJobStore store)
        {
            this.Store = store;
        }

        protected IJobStore Store { get; private set; }

        protected void CancellingJobs()
        {
            if (this.Store != null)
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
        }

        protected void CreateJob()
        {
            if (this.Store != null)
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
        }

        protected void DequeueingJobs()
        {
            if (this.Store != null)
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
        }

        protected void EnqueueJob()
        {
            if (this.Store != null)
            {
                Assert.IsTrue(0 < CreateQueued(new TestIdJob()).Id);
            }
        }

        protected void FinishingJobs()
        {
            if (this.Store != null)
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
        }

        protected void GetJob()
        {
            if (this.Store != null)
            {
                var id = CreateQueued(new TestIdJob()).Id.Value;
                Assert.IsNotNull(Store.GetJob(id));
            }
        }

        protected void GetLatestScheduledJobs()
        {
            if (this.Store != null)
            {
                JobRunner.Instance.Stop(true);

                string a = Guid.NewGuid().ToString();
                string b = Guid.NewGuid().ToString();
                string c = Guid.NewGuid().ToString();
                string d = Guid.NewGuid().ToString();

                var idA1 = CreateSucceeded(new TestIdJob(), DateTime.Parse("2/1/10"), a).Id.Value;
                var idA2 = CreateSucceeded(new TestIdJob(), DateTime.Parse("2/2/10"), a).Id.Value;
                var idA3 = CreateSucceeded(new TestIdJob(), DateTime.Parse("2/3/10"), a).Id.Value;

                var slowA1 = CreateSucceeded(new TestSlowJob(), DateTime.Parse("2/2/10"), a).Id.Value;
                var slowA2 = CreateSucceeded(new TestSlowJob(), DateTime.Parse("2/3/10"), a).Id.Value;

                var slowB1 = CreateSucceeded(new TestSlowJob(), DateTime.Parse("2/2/10"), b).Id.Value;
                var slowB2 = CreateSucceeded(new TestSlowJob(), DateTime.Parse("2/3/10"), b).Id.Value;

                var timeoutC1 = CreateSucceeded(new TestTimeoutJob(), DateTime.Parse("2/3/10"), c).Id.Value;

                var latest = Store.GetLatestScheduledJobs();

                Assert.AreEqual(idA3, latest.Where(r => r.ScheduleName == a && r.JobType == typeof(TestIdJob)).FirstOrDefault().Id);
                Assert.AreEqual(slowA2, latest.Where(r => r.ScheduleName == a && r.JobType == typeof(TestSlowJob)).FirstOrDefault().Id);
                Assert.AreEqual(slowB2, latest.Where(r => r.ScheduleName == b).FirstOrDefault().Id);
                Assert.AreEqual(timeoutC1, latest.Where(r => r.ScheduleName == c).FirstOrDefault().Id);
            }
        }

        protected void TimingOutJobs()
        {
            if (this.Store != null)
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
        }

        protected void UpdateJob()
        {
            if (this.Store != null)
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

        private JobRecord CreateJob(IJob job, DateTime queueDate, JobStatus status, string scheduleName)
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

        private JobRecord CreateQueued(IJob job)
        {
            return CreateJob(job, DateTime.UtcNow, JobStatus.Queued, null);
        }

        private JobRecord CreateSucceeded(IJob job, DateTime queueDate, string scheduleName)
        {
            return CreateJob(job, queueDate, JobStatus.Succeeded, scheduleName);
        }
    }
}
