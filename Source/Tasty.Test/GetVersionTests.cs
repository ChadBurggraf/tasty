using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Build;

namespace Tasty.Test
{
    [TestClass]
    public class GetVersionTests
    {
        [TestMethod]
        public void GetVersion_AssemblyInfoVersion()
        {
            Version version = Assembly.GetAssembly(typeof(GetVersion)).GetName().Version;

            GetVersion task = new GetVersion()
            {
                AssemblyInfoFile = Path.GetFullPath("SolutionVersionInfo.cs")
            };

            Assert.IsTrue(task.Execute());
            Assert.AreEqual(version.Major, task.Major);
            Assert.AreEqual(version.Minor, task.Minor);
            Assert.AreEqual(version.Build, task.Build);
            Assert.AreEqual(version.Revision, task.Revision);
        }

        [TestMethod]
        public void GetVersion_AssemblyVersion()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(GetVersion));
            Version version = assembly.GetName().Version;

            GetVersion task = new GetVersion()
            {
                AssemblyFile = Regex.Replace(assembly.CodeBase, @"^file:///", String.Empty)
            };

            Assert.IsTrue(task.Execute());
            Assert.AreEqual(version.Major, task.Major);
            Assert.AreEqual(version.Minor, task.Minor);
            Assert.AreEqual(version.Build, task.Build);
            Assert.AreEqual(version.Revision, task.Revision);
        }
    }
}
