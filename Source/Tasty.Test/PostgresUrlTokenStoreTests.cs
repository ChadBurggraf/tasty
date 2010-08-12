using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Web.UrlTokens;

namespace Tasty.Test
{
    [TestClass]
    public class PostgresUrlTokenStoreTests : UrlTokenStoreTests
    {
        private static string connectionString = ConfigurationManager.AppSettings["PostgresConnectionString"];

        public PostgresUrlTokenStoreTests()
            : base(!String.IsNullOrEmpty(connectionString) ? new PostgresUrlTokenStore(connectionString) : null)
        {
        }

        [TestMethod]
        public void PostgresUrlTokenStore_CurrentTokenStore()
        {
            base.CurrentTokenStore();
        }

        [TestMethod]
        public void PostgresUrlTokenStore_CreateUrlToken()
        {
            base.CreateUrlToken();
        }

        [TestMethod]
        public void PostgresUrlTokenStore_ExpireUrlToken()
        {
            base.ExpireUrlToken();
        }

        [TestMethod]
        public void PostgresUrlTokenStore_GetUrlToken()
        {
            base.GetUrlToken();
        }

        [TestMethod]
        public void PostgresUrlTokenStore_QueryStringUrlTokenUrlProvider()
        {
            base.QueryStringUrlTokenUrlProvider();
        }
    }
}
