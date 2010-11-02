using System;
using System.Collections.Specialized;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tasty.Web;

namespace Tasty.Test
{
    [TestClass]
    public class HttpContextTests
    {
        [TestMethod]
        public void HttpContextIsFullyQualifiedUrl()
        {
            string url = "http://www.example.com/";
            Assert.IsTrue(HttpContexts.IsFullyQualifiedUrl(url));
            url = "http://www.example.com?ref=http://www.example.com/help";
            Assert.IsTrue(HttpContexts.IsFullyQualifiedUrl(url));
            url = "/?ref=http://www.example.com/help";
            Assert.IsFalse(HttpContexts.IsFullyQualifiedUrl(url));
            url = "/some/path?some=query";
            Assert.IsFalse(HttpContexts.IsFullyQualifiedUrl(url));
            url = "~/page.aspx";
            Assert.IsFalse(HttpContexts.IsFullyQualifiedUrl(url));
        }

        [TestMethod]
        public void HttpContextResolveUrl()
        {
            var httpContext = MockHttpContext(false);
            string url = httpContext.Object.ResolveUrl("~/some/path/file.png");
            Assert.AreEqual("/tasty/some/path/file.png", url);
            url = httpContext.Object.ResolveUrl("~/some/path/file.png", true);
            Assert.AreEqual("http://example.com/tasty/some/path/file.png", url);
            url = httpContext.Object.ResolveUrl("~/some/path/file.png", true, true);
            Assert.AreEqual("https://example.com/tasty/some/path/file.png", url);
            httpContext = MockHttpContext(true);
            url = httpContext.Object.ResolveUrl("~/some/path/file.png", true);
            Assert.AreEqual("https://example.com/tasty/some/path/file.png", url);
        }

        private static Mock<HttpContextBase> MockHttpContext(bool sslRequestUrl)
        {
            var mockContext = new Mock<HttpContextBase>()
            {
                DefaultValue = DefaultValue.Mock
            };

            var mockRequest = new Mock<HttpRequestBase>()
            {
                DefaultValue = DefaultValue.Mock
            };

            NameValueCollection headers = new NameValueCollection();
            headers["Host"] = "example.com";

            UriBuilder requestUrlBuilder = new UriBuilder("http://example.com/default.aspx");

            if (sslRequestUrl)
            {
                requestUrlBuilder.Scheme = Uri.UriSchemeHttps;
                requestUrlBuilder.Port = 443;
            }

            mockRequest.Setup(r => r.ApplicationPath).Returns("/tasty/");
            mockRequest.Setup(r => r.Headers).Returns(headers);
            mockRequest.Setup(r => r.HttpMethod).Returns("GET");
            mockRequest.Setup(r => r.Url).Returns(requestUrlBuilder.Uri);

            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);

            return mockContext;
        }
    }
}
