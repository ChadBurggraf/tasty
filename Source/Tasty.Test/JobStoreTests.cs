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
            if (store == null)
            {
                throw new ArgumentNullException("store", "store cannot be null.");
            }

            this.Store = store;
        }

        protected IJobStore Store { get; private set; }

        protected virtual void DeleteJobs()
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

        protected virtual void GetJobs()
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
        }

        protected virtual void GetLatestScheduledJobs()
        {
            throw new NotImplementedException();
        }

        protected virtual void SaveJobs()
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

        private JobRecord CreateRecord(IJob job, DateTime queueDate, JobStatus status, string scheduleName)
        {
            JobRecord record = job.CreateRecord(queueDate, scheduleName);
            record.Status = status;

            return record;
        }
    }
}
