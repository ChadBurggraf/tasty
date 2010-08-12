using System;
using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Configuration;
using Tasty.Jobs;

namespace Tasty.Test
{
    [TestClass]
    public class SqlServerJobStoreTests : JobStoreTests
    {
        private static string connectionString = ConfigurationManager.AppSettings["SqlServerConnectionString"];

        public SqlServerJobStoreTests()
            : base(!String.IsNullOrEmpty(connectionString) ? new SqlServerJobStore(connectionString) : null)
        {
        }

        [TestMethod]
        public void SqlServerJobStore_DeleteJobs()
        {
            base.DeleteJobs();
        }

        [TestMethod]
        public void SqlServerJobStore_GetJobs()
        {
            base.GetJobs();
        }

        [TestMethod]
        public void SqlServerJobStore_GetLatestScheduledJobs()
        {
            base.GetLatestScheduledJobs();
        }

        [TestMethod]
        public void SqlServerJobStore_SaveJobs()
        {
            base.SaveJobs();
        }
    }
}
