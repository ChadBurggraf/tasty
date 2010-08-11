using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Configuration;
using Tasty.Jobs;

namespace Tasty.Test
{
    [TestClass]
    public class MemoryJobStoreTests : JobStoreTests
    {
        public MemoryJobStoreTests()
            : base(new MemoryJobStore())
        {
        }

        [TestMethod]
        public void MemoryJobStore_DeleteJobs()
        {
            base.DeleteJobs();
        }

        [TestMethod]
        public void MemoryJobStore_GetJobs()
        {
            base.GetJobs();
        }

        [TestMethod]
        public void MemoryJobStore_GetLatestScheduledJobs()
        {
            base.GetLatestScheduledJobs();
        }

        [TestMethod]
        public void MemoryJobStore_SaveJobs()
        {
            base.SaveJobs();
        }
    }
}
