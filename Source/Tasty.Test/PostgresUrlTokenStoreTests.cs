using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Web;

namespace Tasty.Test
{
    [TestClass]
    public class PostgresUrlTokenStoreTests
    {
        private static string ConnectionString = ConfigurationManager.ConnectionStrings["Postgres"].ConnectionString;

        [TestMethod]
        public void PostgresUrlTokenStore_CreateUrlToken()
        {
            UrlTokenTests.Store_CreateUrlToken(new PostgresUrlTokenStore(ConnectionString));
        }

        [TestMethod]
        public void PostgresUrlTokenStore_ExpireUrlToken()
        {
            UrlTokenTests.Store_ExpireUrlToken(new PostgresUrlTokenStore(ConnectionString));
        }

        [TestMethod]
        public void PostgresUrlTokenStore_GetUrlToken()
        {
            UrlTokenTests.Store_GetUrlToken(new PostgresUrlTokenStore(ConnectionString));
        }
    }
}
