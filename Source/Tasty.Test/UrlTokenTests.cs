//-----------------------------------------------------------------------
// <copyright file="UrlTokenTests.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tasty.Web.UrlTokens;

    /// <summary>
    /// URL token tests.
    /// </summary>
    [TestClass]
    public class UrlTokenTests
    {
        /// <summary>
        /// Is expired tests.
        /// </summary>
        [TestMethod]
        public void UrlTokenIsExpired()
        {
            IUrlTokenStore store = new HttpCacheUrlTokenStore();

            QueryStringUrlTokenUrlProvider<TestIdUrlToken> provider = new QueryStringUrlTokenUrlProvider<TestIdUrlToken>()
            {
                QueryStringKey = "t",
                Url = new Uri("http://tastycodes.com/")
            };

            TestIdUrlToken token = new TestIdUrlToken();
            Uri url = provider.UrlFromToken(token, store);
            token = provider.TokenFromUrl(url, store);
            Assert.IsFalse(token.IsExpired);

            token = new TestIdUrlToken()
            {
                Expires = DateTime.UtcNow.AddHours(-1)
            };

            url = provider.UrlFromToken(token, store);
            token = provider.TokenFromUrl(url, store);
            Assert.IsTrue(token.IsExpired);
        }
    }
}
