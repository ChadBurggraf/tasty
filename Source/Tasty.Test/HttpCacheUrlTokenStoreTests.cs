using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Web;

namespace Tasty.Test
{
    [TestClass]
    public class HttpCacheUrlTokenStoreTests
    {
        [TestMethod]
        public void HttpCacheUrlTokenStore_CreateUrlToken()
        {
            UrlTokenTests.Store_CreateUrlToken(new HttpCacheUrlTokenStore());
        }

        [TestMethod]
        public void HttpCacheUrlTokenStore_ExpireUrlToken()
        {
            UrlTokenTests.Store_ExpireUrlToken(new HttpCacheUrlTokenStore());
        }

        [TestMethod]
        public void HttpCacheUrlTokenStore_GetUrlToken()
        {
            UrlTokenTests.Store_GetUrlToken(new HttpCacheUrlTokenStore());
        }
    }
}
