//-----------------------------------------------------------------------
// <copyright file="HttpTests.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Tasty.Configuration;
    using Tasty.Http;

    /// <summary>
    /// HTTP tests.
    /// </summary>
    [TestClass]
    public class HttpTests
    {
        /// <summary>
        /// Creates a mock HTTP context with the given request URL.
        /// </summary>
        /// <param name="url">The request URL to mock the context for.</param>
        /// <returns>A mock HTTP context.</returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "No.")]
        public static HttpContextBase MockHttpContext(string url)
        {
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            var response = new Mock<HttpResponseBase>();

            request.Setup(r => r.ApplicationPath).Returns("~/");
            request.Setup(r => r.AppRelativeCurrentExecutionFilePath).Returns("~/");
            request.Setup(r => r.PathInfo).Returns(String.Empty);
            request.Setup(r => r.Url).Returns(new Uri(url));

            response.Setup(r => r.ApplyAppPathModifier(It.IsAny<string>())).Returns((string virtualPath) => virtualPath);

            context.Setup(c => c.Request).Returns(request.Object);
            context.Setup(c => c.Response).Returns(response.Object);

            return context.Object;
        }

        /// <summary>
        /// Redirect match rules tests.
        /// </summary>
        [TestMethod]
        public void HttpRedirectMatchRules()
        {
            HttpRedirectRuleElement[] rules = new HttpRedirectRuleElement[] 
            {
                new HttpRedirectRuleElement() { Pattern = @"^(https?://)www\.(.*)", RedirectsTo = "$1$2" },
                new HttpRedirectRuleElement() { Pattern = @"^(.*)?default\.aspx(.*)", RedirectsTo = "$1index.html$2" }
            };

            HttpRedirectRuleMatcher matcher = new HttpRedirectRuleMatcher();

            Uri uri = new Uri("https://www.virtualkeychain.com/vault.html");
            Assert.AreEqual("https://virtualkeychain.com/vault.html", matcher.Match(uri, rules).RedirectResult);

            uri = new Uri("https://virtualkeychain.com/default.aspx?name=home");
            Assert.AreEqual("https://virtualkeychain.com/index.html?name=home", matcher.Match(uri, rules).RedirectResult);

            uri = new Uri("http://localhost/vkc/default.aspx?name=home");
            Assert.AreEqual("http://localhost/vkc/index.html?name=home", matcher.Match(uri, rules).RedirectResult);
        }
    }
}
