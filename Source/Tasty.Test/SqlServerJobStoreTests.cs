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
    [TestClass]
    public class SqlServerJobStoreTests : JobStoreTests
    {
        public SqlServerJobStoreTests()
            : base(new SqlServerJobStore(ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString))
        {
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            Bootstrapper.EnsureTestDatabase();
        }

        [TestMethod]
        public void SqlServerJobStore_CancellingJobs()
        {
            base.CancellingJobs();
        }

        [TestMethod]
        public void SqlServerJobStore_CreateJob()
        {
            base.CreateJob();
        }

        [TestMethod]
        public void SqlServerJobStore_DequeueingJobs()
        {
            base.DequeueingJobs();
        }

        [TestMethod]
        public void SqlServerJobStore_EnqueueJob()
        {
            base.EnqueueJob();
        }

        [TestMethod]
        public void SqlServerJobStore_FinishingJobs()
        {
            base.FinishingJobs();
        }

        [TestMethod]
        public void SqlServerJobStore_GetJob()
        {
            base.GetJob();
        }

        [TestMethod]
        public void SqlServerJobStore_GetLatestScheduledJobs()
        {
            base.GetLatestScheduledJobs();
        }

        [TestMethod]
        public void SqlServerJobStore_TimingOutJobs()
        {
            base.TimingOutJobs();
        }

        [TestMethod]
        public void SqlServerJobStore_UpdateJob()
        {
            base.UpdateJob();
        }
    }
}
