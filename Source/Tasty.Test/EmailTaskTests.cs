//-----------------------------------------------------------------------
// <copyright file="EmailTaskTests.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.IO;
    using Microsoft.Build.BuildEngine;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Email MSBuild task tests.
    /// </summary>
    [TestClass]
    public class EmailTaskTests
    {
        /// <summary>
        /// MSBuild tests.
        /// </summary>
        [TestMethod]
        public void EmailTaskMSBuild()
        {
            const string ProjectFile = "Email.proj";

            if (File.Exists(ProjectFile))
            {
                Assert.IsTrue(Engine.GlobalEngine.BuildProjectFile(ProjectFile));
            }
        }
    }
}
