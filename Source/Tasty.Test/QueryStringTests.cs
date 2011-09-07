//-----------------------------------------------------------------------
// <copyright file="QueryStringTests.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tasty.Web;

    /// <summary>
    /// Query string tests.
    /// </summary>
    [TestClass]
    public sealed class QueryStringTests
    {
        /// <summary>
        /// ToOrderedDescendingString tests.
        /// </summary>
        [TestMethod]
        public void QueryStringToOrderedDescendingString()
        {
            QueryString query = QueryString.Parse("media=software&entity=software&attribute=softwareDeveloper&term=Apple");
            Assert.AreEqual("term=Apple&media=software&entity=software&attribute=softwareDeveloper", query.ToOrderedDescendingString());

            query = QueryString.Parse("media=software&term=Google&entity=software&attribute=softwareDeveloper&term=Apple");
            Assert.AreEqual("term=Google&term=Apple&media=software&entity=software&attribute=softwareDeveloper", query.ToOrderedDescendingString());
        }

        /// <summary>
        /// ToOrderedString tests.
        /// </summary>
        [TestMethod]
        public void QueryStringToOrderedString()
        {
            QueryString query = QueryString.Parse("media=software&entity=software&attribute=softwareDeveloper&term=Apple");
            Assert.AreEqual("attribute=softwareDeveloper&entity=software&media=software&term=Apple", query.ToOrderedString());

            query = QueryString.Parse("media=software&term=Google&entity=software&attribute=softwareDeveloper&term=Apple");
            Assert.AreEqual("attribute=softwareDeveloper&entity=software&media=software&term=Apple&term=Google", query.ToOrderedString());
        }
    }
}
