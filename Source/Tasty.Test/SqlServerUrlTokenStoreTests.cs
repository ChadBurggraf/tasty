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
    public class SqlServerUrlTokenStoreTests
    {
        private static string ConnectionString = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;

        [TestMethod]
        public void SqlServerUrlTokenStore_CreateUrlToken()
        {
            UrlTokenTests.Store_CreateUrlToken(new SqlServerUrlTokenStore(ConnectionString));
        }

        [TestMethod]
        public void SqlServerUrlTokenStore_ExpireUrlToken()
        {
            UrlTokenTests.Store_ExpireUrlToken(new SqlServerUrlTokenStore(ConnectionString));
        }

        [TestMethod]
        public void SqlServerUrlTokenStore_GetUrlToken()
        {
            UrlTokenTests.Store_GetUrlToken(new SqlServerUrlTokenStore(ConnectionString));
        }
    }
}
