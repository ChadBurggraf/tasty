//-----------------------------------------------------------------------
// <copyright file="SQLiteUrlTokenStoreTests.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tasty.Web.UrlTokens;

    /// <summary>
    /// SQLite URL token store tests.
    /// </summary>
    [TestClass]
    public class SQLiteUrlTokenStoreTests : UrlTokenStoreTests
    {
        /// <summary>
        /// Initializes a new instance of the SQLiteUrlTokenStoreTests class.
        /// </summary>
        public SQLiteUrlTokenStoreTests()
            : base(new SQLiteUrlTokenStore("data source=SQLiteUrlTokenStoreTests.s3db"))
        {
        }

        /// <summary>
        /// Current token store tests.
        /// </summary>
        [TestMethod]
        public void SQLiteUrlTokenStoreCurrentTokenStore()
        {
            CurrentTokenStore();
        }

        /// <summary>
        /// Create URL token tests.
        /// </summary>
        [TestMethod]
        public void SQLiteUrlTokenStoreCreateUrlToken()
        {
            CreateUrlToken();
        }

        /// <summary>
        /// Expire URL token tests.
        /// </summary>
        [TestMethod]
        public void SQLiteUrlTokenStoreExpireUrlToken()
        {
            ExpireUrlToken();
        }

        /// <summary>
        /// Get URL token tests.
        /// </summary>
        [TestMethod]
        public void SQLiteUrlTokenStoreGetUrlToken()
        {
            GetUrlToken();
        }

        /// <summary>
        /// Query string URL token URL provider tests.
        /// </summary>
        [TestMethod]
        public void SQLiteUrlTokenStoreQueryStringUrlTokenUrlProvider()
        {
            QueryStringUrlTokenUrlProvider();
        }
    }
}