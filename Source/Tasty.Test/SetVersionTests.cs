using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Build;

namespace Tasty.Test
{
    [TestClass]
    public class SetVersionTests
    {
        [TestMethod]
        public void SetVersion_SetVersion()
        {
            string version = Assembly.GetAssembly(typeof(SetVersion)).GetName().Version.ToString(4);

            SetVersion task = new SetVersion()
            {
                Files = new TaskItem[] { new TaskItem("AssemblyInfo1.txt"), new TaskItem("AssemblyInfo2.txt") },
                Version = version
            };

            task.Execute();

            Assert.IsTrue(File.ReadAllText("AssemblyInfo1.txt").Contains(version));
            Assert.IsTrue(File.ReadAllText("AssemblyInfo2.txt").Contains(version));
        }
    }
}
