//-----------------------------------------------------------------------
// <copyright file="SqlServerClrTests.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tasty.SqlServer;

    /// <summary>
    /// SQL Server CLR tests.
    /// </summary>
    [TestClass]
    public class SqlServerClrTests
    {
        /// <summary>
        /// Assembly import script tests.
        /// </summary>
        [TestMethod]
        public void SqlServerClrAssemblyImportScript()
        {
            string tasty = Assembly.GetAssembly(typeof(AssemblyImportScript)).Location;
            string script = AssemblyImportScript.Get();
            Assert.IsTrue(script.Contains("@TastyAssemblyPath = '" + tasty + "'"));
        }

        /// <summary>
        /// String set contains any tests.
        /// </summary>
        [TestMethod]
        public void SqlServerClrStringSetContainsAny()
        {
            string a = String.Empty, b = String.Empty, separator = String.Empty;
            Assert.IsTrue(ClrFunctions.StringSetContainsAny(a, b, separator).IsTrue);

            b = "Administrators";
            Assert.IsFalse(ClrFunctions.StringSetContainsAny(a, b, separator).IsTrue);

            a = "Developers\nAdministrators";
            b = "Power Users\nUsers";
            Assert.IsFalse(ClrFunctions.StringSetContainsAny(a, b, separator).IsTrue);

            b += "\nAdministrators";
            Assert.IsTrue(ClrFunctions.StringSetContainsAny(a, b, separator).IsTrue);

            b = String.Empty;
            Assert.IsFalse(ClrFunctions.StringSetContainsAny(a, b, separator).IsTrue);

            a = "ApplicationAdmin";
            b = "ApplicationUser\n?";
            Assert.IsFalse(ClrFunctions.StringSetContainsAny(a, b, separator).IsTrue);
            Assert.IsFalse(ClrFunctions.StringSetContainsAny(a, b, "\n").IsTrue);
        }
    }
}