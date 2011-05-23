//-----------------------------------------------------------------------
// <copyright file="HttpCacheUrlTokenStoreTests.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tasty.Web.UrlTokens;

    /// <summary>
    /// HTTP-cache URL token store tests.
    /// </summary>
    [TestClass]
    public class HttpCacheUrlTokenStoreTests : UrlTokenStoreTests
    {
        /// <summary>
        /// Initializes a new instance of the HttpCacheUrlTokenStoreTests class.
        /// </summary>
        public HttpCacheUrlTokenStoreTests()
            : base(new HttpCacheUrlTokenStore())
        {
        }

        /// <summary>
        /// Current token store tests.
        /// </summary>
        [TestMethod]
        public void HttpCacheUrlTokenStoreCurrentTokenStore()
        {
            CurrentTokenStore();
        }

        /// <summary>
        /// Create URL token tests.
        /// </summary>
        [TestMethod]
        public void HttpCacheUrlTokenStoreCreateUrlToken()
        {
            CreateUrlToken();
        }

        /// <summary>
        /// Expire URL token tests.
        /// </summary>
        [TestMethod]
        public void HttpCacheUrlTokenStoreExpireUrlToken()
        {
            ExpireUrlToken();
        }

        /// <summary>
        /// Get URL token tests.
        /// </summary>
        [TestMethod]
        public void HttpCacheUrlTokenStoreGetUrlToken()
        {
            GetUrlToken();
        }

        /// <summary>
        /// Query string URL token provider tests.
        /// </summary>
        [TestMethod]
        public void HttpCacheUrlTokenStoreQueryStringUrlTokenUrlProvider()
        {
            QueryStringUrlTokenUrlProvider();
        }
    }
}
