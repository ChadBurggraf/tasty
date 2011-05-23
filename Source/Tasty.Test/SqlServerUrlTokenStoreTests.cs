//-----------------------------------------------------------------------
// <copyright file="SqlServerUrlTokenStoreTests.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tasty.Web.UrlTokens;

    /// <summary>
    /// SQL Server URL token store tests.
    /// </summary>
    [TestClass]
    public class SqlServerUrlTokenStoreTests : UrlTokenStoreTests
    {
        private static string connectionString = ConfigurationManager.AppSettings["SqlServerConnectionString"];

        /// <summary>
        /// Initializes a new instance of the SqlServerUrlTokenStoreTests class.
        /// </summary>
        public SqlServerUrlTokenStoreTests()
            : base(!String.IsNullOrEmpty(connectionString) ? new SqlServerUrlTokenStore(connectionString) : null)
        {
        }

        /// <summary>
        /// Current token store tests.
        /// </summary>
        [TestMethod]
        public void SqlServerUrlTokenStoreCurrentTokenStore()
        {
            CurrentTokenStore();
        }

        /// <summary>
        /// Create URL token tests.
        /// </summary>
        [TestMethod]
        public void SqlServerUrlTokenStoreCreateUrlToken()
        {
            CreateUrlToken();
        }

        /// <summary>
        /// Expire URL token tests.
        /// </summary>
        [TestMethod]
        public void SqlServerUrlTokenStoreExpireUrlToken()
        {
            ExpireUrlToken();
        }

        /// <summary>
        /// Get URL token tests.
        /// </summary>
        [TestMethod]
        public void SqlServerUrlTokenStoreGetUrlToken()
        {
            GetUrlToken();
        }

        /// <summary>
        /// Query string URL token URL provider tests.
        /// </summary>
        [TestMethod]
        public void SqlServerUrlTokenStoreQueryStringUrlTokenUrlProvider()
        {
            QueryStringUrlTokenUrlProvider();
        }
    }
}
