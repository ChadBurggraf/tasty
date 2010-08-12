using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Web.UrlTokens;

namespace Tasty.Test
{
    [TestClass]
    public class HttpCacheUrlTokenStoreTests : UrlTokenStoreTests
    {
        public HttpCacheUrlTokenStoreTests()
            : base(new HttpCacheUrlTokenStore())
        {
        }

        [TestMethod]
        public void HttpCacheUrlTokenStore_CurrentTokenStore()
        {
            base.CurrentTokenStore();
        }

        [TestMethod]
        public void HttpCacheUrlTokenStore_CreateUrlToken()
        {
            base.CreateUrlToken();
        }

        [TestMethod]
        public void HttpCacheUrlTokenStore_ExpireUrlToken()
        {
            base.ExpireUrlToken();
        }

        [TestMethod]
        public void HttpCacheUrlTokenStore_GetUrlToken()
        {
            base.GetUrlToken();
        }

        [TestMethod]
        public void HttpCacheUrlTokenStore_QueryStringUrlTokenUrlProvider()
        {
            base.QueryStringUrlTokenUrlProvider();
        }
    }
}
