using System;
using System.IO;
using Microsoft.Build.BuildEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Build;

namespace Tasty.Test
{
    [TestClass]
    public class EmailTaskTests
    {
        [TestMethod]
        public void EmailTask_MsBuild()
        {
            const string ProjectFile = "Email.proj";

            if (File.Exists(ProjectFile))
            {
                Assert.IsTrue(Engine.GlobalEngine.BuildProjectFile(ProjectFile));
            }
        }
    }
}
