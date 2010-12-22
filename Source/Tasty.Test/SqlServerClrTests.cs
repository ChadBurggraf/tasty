﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.SqlServer;

namespace Tasty.Test
{
    [TestClass]
    public class SqlServerClrTests
    {
        [TestMethod]
        public void SqlServerClr_AssemblyImportScript()
        {
            string tasty = Assembly.GetAssembly(typeof(AssemblyImportScript)).Location;
            string script = AssemblyImportScript.Get();
            Assert.IsTrue(script.Contains("@TastyAssemblyPath = '" + tasty + "'"));
        }

        [TestMethod]
        public void SqlServerClr_StringSetContainsAny()
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
