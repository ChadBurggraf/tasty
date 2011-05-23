//-----------------------------------------------------------------------
// <copyright file="SetVersionTests.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.IO;
    using System.Reflection;
    using Microsoft.Build.Utilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tasty.Build;

    /// <summary>
    /// Set version tests.
    /// </summary>
    [TestClass]
    public class SetVersionTests
    {
        /// <summary>
        /// Set version tests.
        /// </summary>
        [TestMethod]
        public void SetVersionSetVersion()
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
