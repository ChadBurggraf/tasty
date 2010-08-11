using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Configuration;
using Tasty.Jobs;

namespace Tasty.Test
{
    [TestClass]
    public class JobTests
    {
        [TestMethod]
        public void Jobs_RunningJobsFlush()
        {
            RunningJobs runs = new RunningJobs();

            if (File.Exists(runs.PersistencePath))
            {
                File.Delete(runs.PersistencePath);
            }

            runs.Add(new JobRun(1, new TestIdJob()));
            runs.Add(new JobRun(2, new TestIdJob()));
            runs.Flush();

            Assert.IsTrue(File.Exists(runs.PersistencePath));
        }
    }
}
