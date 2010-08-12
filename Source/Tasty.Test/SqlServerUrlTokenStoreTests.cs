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
    public class SqlServerUrlTokenStoreTests : UrlTokenStoreTests
    {
        private static string connectionString = ConfigurationManager.AppSettings["SqlServerConnectionString"];

        public SqlServerUrlTokenStoreTests()
            : base(!String.IsNullOrEmpty(connectionString) ? new SqlServerUrlTokenStore(connectionString) : null)
        {
        }

        [TestMethod]
        public void SqlServerUrlTokenStore_CurrentTokenStore()
        {
            base.CurrentTokenStore();
        }

        [TestMethod]
        public void SqlServerUrlTokenStore_CreateUrlToken()
        {
            base.CreateUrlToken();
        }

        [TestMethod]
        public void SqlServerUrlTokenStore_ExpireUrlToken()
        {
            base.ExpireUrlToken();
        }

        [TestMethod]
        public void SqlServerUrlTokenStore_GetUrlToken()
        {
            base.GetUrlToken();
        }

        [TestMethod]
        public void SqlServerUrlTokenStore_QueryStringUrlTokenUrlProvider()
        {
            base.QueryStringUrlTokenUrlProvider();
        }
    }
}
