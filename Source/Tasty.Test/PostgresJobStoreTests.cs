using System;
using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Configuration;
using Tasty.Jobs;

namespace Tasty.Test
{
    [TestClass]
    public class PostgresJobStoreTests : JobStoreTests
    {
        private static string connectionString = ConfigurationManager.AppSettings["PostgresConnectionString"];

        public PostgresJobStoreTests()
            : base(!String.IsNullOrEmpty(connectionString) ? new PostgresJobStore(connectionString) : null)
        {
        }

        [TestMethod]
        public void PostgresJobStore_DeleteJobs()
        {
            base.DeleteJobs();
        }

        [TestMethod]
        public void PostgresJobStore_GetJobs()
        {
            base.GetJobs();
        }

        [TestMethod]
        public void PostgresJobStore_GetLatestScheduledJobs()
        {
            base.GetLatestScheduledJobs();
        }

        [TestMethod]
        public void PostgresJobStore_SaveJobs()
        {
            base.SaveJobs();
        }
    }
}
