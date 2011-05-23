//-----------------------------------------------------------------------
// <copyright file="GetVersionTests.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tasty.Build;

    /// <summary>
    /// Get version tests.
    /// </summary>
    [TestClass]
    public class GetVersionTests
    {
        /// <summary>
        /// Assembly info get version tests.
        /// </summary>
        [TestMethod]
        public void GetVersionAssemblyInfoVersion()
        {
            Version version = Assembly.GetAssembly(typeof(GetVersion)).GetName().Version;

            GetVersion task = new GetVersion()
            {
                AssemblyInfoFile = Path.GetFullPath("SolutionInfo.cs")
            };

            Assert.IsTrue(task.Execute());
            Assert.AreEqual(version.Major, task.Major);
            Assert.AreEqual(version.Minor, task.Minor);
            Assert.AreEqual(version.Build, task.Build);
            Assert.AreEqual(version.Revision, task.Revision);
        }

        /// <summary>
        /// Assembly get version tests.
        /// </summary>
        [TestMethod]
        public void GetVersionAssemblyVersion()
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
