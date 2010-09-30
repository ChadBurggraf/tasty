using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Web.UrlTokens;

namespace Tasty.Test
{
    [TestClass]
    public class UrlTokenTests
    {
        [TestMethod]
        public void UrlToken_IsExpired()
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
