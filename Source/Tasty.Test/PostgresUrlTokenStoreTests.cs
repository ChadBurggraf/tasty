//-----------------------------------------------------------------------
// <copyright file="PostgresUrlTokenStoreTests.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tasty.Web.UrlTokens;

    /// <summary>
    /// PostgreSQL URL token store tests.
    /// </summary>
    [TestClass]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class PostgresUrlTokenStoreTests : UrlTokenStoreTests
    {
        private static string connectionString = ConfigurationManager.AppSettings["PostgresConnectionString"];

        /// <summary>
        /// Initializes a new instance of the PostgresUrlTokenStoreTests class.
        /// </summary>
        public PostgresUrlTokenStoreTests()
            : base(!String.IsNullOrEmpty(connectionString) ? new PostgresUrlTokenStore(connectionString) : null)
        {
        }

        /// <summary>
        /// Current token store tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void PostgresUrlTokenStoreCurrentTokenStore()
        {
            CurrentTokenStore();
        }

        /// <summary>
        /// Create URL token tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void PostgresUrlTokenStoreCreateUrlToken()
        {
            CreateUrlToken();
        }

        /// <summary>
        /// Expire URL token tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void PostgresUrlTokenStoreExpireUrlToken()
        {
            ExpireUrlToken();
        }

        /// <summary>
        /// Gets URL token tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void PostgresUrlTokenStoreGetUrlToken()
        {
            GetUrlToken();
        }

        /// <summary>
        /// Querystring URL token provider tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void PostgresUrlTokenStoreQueryStringUrlTokenUrlProvider()
        {
            QueryStringUrlTokenUrlProvider();
        }
    }
}
