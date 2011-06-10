//-----------------------------------------------------------------------
// <copyright file="CollectionTests.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Collection tests.
    /// </summary>
    [TestClass]
    public sealed class CollectionTests
    {
        /// <summary>
        /// Covariance tests.
        /// </summary>
        [TestMethod]
        public void CollectionsCovariance()
        {
            int[] x = new int[] { 1, 3, 3, 5 };
            int[] y = new int[] { 12, 12, 11, 7 };
            Assert.AreEqual(-2.5, x.Covariance(y));
        }

        /// <summary>
        /// Standard deviation tests.
        /// </summary>
        [TestMethod]
        public void CollectionsStandardDeviation()
        {
            int[] ints = new int[] { 2, 4, 4, 4, 5, 5, 7, 9 };
            Assert.AreEqual(2, ints.StandardDeviation());
            Assert.AreEqual(0, Collections.StandardDeviation<int>(null));
        }

        /// <summary>
        /// Variance tests.
        /// </summary>
        [TestMethod]
        public void CollectionsVariance()
        {
            int[] ints = new int[] { 2, 4, 4, 4, 5, 5, 7, 9 };
            Assert.AreEqual(4, ints.Variance());
            Assert.AreEqual(0, Collections.Variance<int>(null));
        }
    }
}
