using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Jobs;

namespace Tasty.Test
{
    [TestClass]
    public class PostgresJobStoreTests : JobStoreTests
    {
        public PostgresJobStoreTests()
            : base(new PostgresJobStore(ConfigurationManager.ConnectionStrings["Postgres"].ConnectionString))
        {
        }

        [TestMethod]
        public void PostgresJobStore_CancellingJobs()
        {
            base.CancellingJobs();
        }

        [TestMethod]
        public void PostgresJobStore_CreateJob()
        {
            base.CreateJob();
        }

        [TestMethod]
        public void PostgresJobStore_DequeueingJobs()
        {
            base.DequeueingJobs();
        }

        [TestMethod]
        public void PostgresJobStore_EnqueueJob()
        {
            base.EnqueueJob();
        }

        [TestMethod]
        public void PostgresJobStore_FinishingJobs()
        {
            base.FinishingJobs();
        }

        [TestMethod]
        public void PostgresJobStore_GetJob()
        {
            base.GetJob();
        }

        [TestMethod]
        public void PostgresJobStore_GetLatestScheduledJobs()
        {
            base.GetLatestScheduledJobs();
        }

        [TestMethod]
        public void PostgresJobStore_TimingOutJobs()
        {
            base.TimingOutJobs();
        }

        [TestMethod]
        public void PostgresJobStore_UpdateJob()
        {
            base.UpdateJob();
        }
    }
}
