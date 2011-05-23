//-----------------------------------------------------------------------
// <copyright file="SqlDateTimeAssert.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Provides an assert for comparing SQL date/times.
    /// </summary>
    internal static class SqlDateTimeAssert
    {
        /// <summary>
        /// Asserts the given values are equal.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        public static void AreEqual(DateTime? expected, DateTime? actual)
        {
            if (expected != null && actual != null)
            {
                var e = new DateTime(expected.Value.Year, expected.Value.Month, expected.Value.Day, expected.Value.Hour, expected.Value.Minute, expected.Value.Second);
                var a = new DateTime(actual.Value.Year, actual.Value.Month, actual.Value.Day, actual.Value.Hour, actual.Value.Minute, actual.Value.Second);

                Assert.AreEqual(e, a);
            }
            else if (expected != null)
            {
                Assert.Fail("Expected: <{0}>. Actual: <null>.", expected);
            }
            else if (actual != null)
            {
                Assert.Fail("Expected: <null>. Actual: <{0}>.", actual);
            }
        }
    }
}
