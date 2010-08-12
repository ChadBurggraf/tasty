using System;
using System.Linq;
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

        protected virtual void DeleteJobs()
        {
            if (this.Store != null)
            {
                IJobStoreTransaction trans;

                var job1 = this.CreateRecord(new TestIdJob(), DateTime.UtcNow, JobStatus.Queued, null);
                this.Store.SaveJob(job1);
                Assert.IsNotNull(this.Store.GetJob(job1.Id.Value));
                this.Store.DeleteJob(job1.Id.Value);
                Assert.IsNull(this.Store.GetJob(job1.Id.Value));

                var job2 = this.CreateRecord(new TestIdJob(), DateTime.UtcNow, JobStatus.Queued, null);
                this.Store.SaveJob(job2);
                trans = this.Store.StartTransaction();
                this.Store.DeleteJob(job2.Id.Value, trans);
                trans.Rollback();
                Assert.IsNotNull(this.Store.GetJob(job2.Id.Value));

                var job3 = this.CreateRecord(new TestIdJob(), DateTime.UtcNow, JobStatus.Queued, null);
                this.Store.SaveJob(job3);
                trans = this.Store.StartTransaction();
                this.Store.DeleteJob(job3.Id.Value, trans);
                trans.Commit();
                Assert.IsNull(this.Store.GetJob(job3.Id.Value));
            }
        }

        protected virtual void GetJobs()
        {
            if (this.Store != null)
            {
                int queuedCount = this.Store.GetJobs(JobStatus.Queued, 0).Count();
                int finishedCount = this.Store.GetJobs(JobStatus.Succeeded, 0).Count();

                var job1 = this.CreateRecord(new TestIdJob(), DateTime.UtcNow, JobStatus.Queued, null);
                var job2 = this.CreateRecord(new TestIdJob(), DateTime.UtcNow, JobStatus.Succeeded, null);

                this.Store.SaveJob(job1);
                this.Store.SaveJob(job2);

                var jobs = this.Store.GetJobs(new int[] { job1.Id.Value, job2.Id.Value });

                Assert.AreEqual(2, jobs.Count());
                Assert.IsTrue(jobs.Any(j => j.Id.Value == job1.Id.Value));
                Assert.IsTrue(jobs.Any(j => j.Id.Value == job2.Id.Value));

                Assert.AreEqual(queuedCount + 1, this.Store.GetJobs(JobStatus.Queued, 0).Count());
                Assert.AreEqual(finishedCount + 1, this.Store.GetJobs(JobStatus.Succeeded, 0).Count());
                Assert.IsNotNull(this.Store.GetJob(job1.Id.Value));
                Assert.AreEqual(0, this.Store.GetJobs(new int[0]).Count());
            }
        }

        protected virtual void GetLatestScheduledJobs()
        {
            if (this.Store != null)
            {
                JobRunner.GetInstance(this.Store).Stop(true);

                string a = Guid.NewGuid().ToString();
                string b = Guid.NewGuid().ToString();
                string c = Guid.NewGuid().ToString();
                string d = Guid.NewGuid().ToString();

                var idA1 = this.CreateSucceeded(new TestIdJob(), DateTime.Parse("2/1/10"), a).Id.Value;
                var idA2 = this.CreateSucceeded(new TestIdJob(), DateTime.Parse("2/2/10"), a).Id.Value;
                var idA3 = this.CreateSucceeded(new TestIdJob(), DateTime.Parse("2/3/10"), a).Id.Value;

                var slowA1 = this.CreateSucceeded(new TestSlowJob(), DateTime.Parse("2/2/10"), a).Id.Value;
                var slowA2 = this.CreateSucceeded(new TestSlowJob(), DateTime.Parse("2/3/10"), a).Id.Value;

                var slowB1 = this.CreateSucceeded(new TestSlowJob(), DateTime.Parse("2/2/10"), b).Id.Value;
                var slowB2 = this.CreateSucceeded(new TestSlowJob(), DateTime.Parse("2/3/10"), b).Id.Value;

                var timeoutC1 = this.CreateSucceeded(new TestTimeoutJob(), DateTime.Parse("2/3/10"), c).Id.Value;

                var latest = Store.GetLatestScheduledJobs();

                string idJobType = JobRecord.JobTypeString(typeof(TestIdJob));
                string slowJobType = JobRecord.JobTypeString(typeof(TestSlowJob));

                Assert.AreEqual(idA3, latest.Where(r => r.ScheduleName == a && r.JobType == idJobType).FirstOrDefault().Id);
                Assert.AreEqual(slowA2, latest.Where(r => r.ScheduleName == a && r.JobType == slowJobType).FirstOrDefault().Id);
                Assert.AreEqual(slowB2, latest.Where(r => r.ScheduleName == b).FirstOrDefault().Id);
                Assert.AreEqual(timeoutC1, latest.Where(r => r.ScheduleName == c).FirstOrDefault().Id);
            }
        }

        protected virtual void SaveJobs()
        {
            if (this.Store != null)
            {
                IJobStoreTransaction trans;

                var job1 = this.CreateRecord(new TestIdJob(), DateTime.UtcNow, JobStatus.Queued, null);
                Assert.IsNull(job1.Id);
                this.Store.SaveJob(job1);
                Assert.IsNotNull(job1.Id);
                Assert.IsNotNull(this.Store.GetJob(job1.Id.Value));

                var job2 = this.CreateRecord(new TestIdJob(), DateTime.UtcNow, JobStatus.Queued, null);
                trans = this.Store.StartTransaction();
                this.Store.SaveJob(job2, trans);
                trans.Rollback();

                if (job2.Id != null)
                {
                    Assert.IsNull(this.Store.GetJob(job2.Id.Value));
                }

                var job3 = this.CreateRecord(new TestIdJob(), DateTime.Now, JobStatus.Queued, null);
                trans = this.Store.StartTransaction();
                this.Store.SaveJob(job3, trans);
                trans.Commit();
                Assert.IsNotNull(job3.Id);
                Assert.IsNotNull(this.Store.GetJob(job3.Id.Value));
            }
        }

        private JobRecord CreateRecord(IJob job, DateTime queueDate, JobStatus status, string scheduleName)
        {
            JobRecord record = job.CreateRecord(queueDate, scheduleName);
            record.Status = status;

            return record;
        }

        private JobRecord CreateSucceeded(IJob job, DateTime queueDate, string scheduleName)
        {
            JobRecord record = this.CreateRecord(job, queueDate, JobStatus.Succeeded, scheduleName);
            this.Store.SaveJob(record);

            return record;
        }
    }
}
