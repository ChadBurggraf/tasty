//-----------------------------------------------------------------------
// <copyright file="HttpRedirectModule.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Http
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Caching;
    using Tasty.Configuration;

    /// <summary>
    /// Implements <see cref="IHttpModule"/> to do simple regular-expression based HTTP redirection.
    /// </summary>
    public class HttpRedirectModule : IHttpModule
    {
        #region Private Fields

        private const string MatchCacheKey = "Tasty.Http.HttpRedirectModule.Cache";

        #endregion

        #region Construction

        /// <summary>
        /// Initializes static members of the HttpRedirectModule class.
        /// </summary>
        static HttpRedirectModule()
        {
            RuleMatcher = new HttpRedirectRuleMatcher();
        }

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Gets the rule matcher used when matching rules.
        /// </summary>
        public static HttpRedirectRuleMatcher RuleMatcher { get; private set; }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Disposes of any unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Gets a rule matching the given HTTP context from the configuration.
        /// Will cache the results of the match.
        /// </summary>
        /// <param name="httpContext">The HTTP context to match.</param>
        /// <returns>The result of the match.</returns>
        public virtual HttpRedirectRuleMatch GetMatchingRule(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext", "httpContext cannot be null.");
            }

            HttpRedirectRuleMatch ruleMatch = null;

            lock (httpContext.Cache)
            {
                Dictionary<string, HttpRedirectRuleMatch> cache = (Dictionary<string, HttpRedirectRuleMatch>)(httpContext.Cache[MatchCacheKey] ?? new Dictionary<string, HttpRedirectRuleMatch>());
                string key = httpContext.Request.Url.AbsoluteUri.ToUpperInvariant();
                bool cacheUpdated = false;

                if (cache.ContainsKey(key))
                {
                    ruleMatch = cache[key];
                }
                else
                {
                    ruleMatch = RuleMatcher.Match(httpContext.Request.Url, TastySettings.Section.Http.Redirects);
                    cache[key] = ruleMatch;
                    cacheUpdated = true;
                }

                if (cacheUpdated)
                {
                    httpContext.Cache.Remove(MatchCacheKey);
                    httpContext.Cache.Add(
                        MatchCacheKey,
                        cache,
                        null,
                        DateTime.Now.AddMinutes(15),
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.Normal,
                        null);
                }
            }

            return ruleMatch;
        }

        /// <summary>
        /// Initializes the module.
        /// </summary>
        /// <param name="context">The <see cref="HttpApplication"/> that is handling the current request.</param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(this.ContextBeginRequest);
        }

        /// <summary>
        /// Redirects the given HTTP context to the redirect result of the given rule match.
        /// </summary>
        /// <param name="httpContext">The HTTP context to redirect.</param>
        /// <param name="ruleMatch">The rule match to redirect with.</param>
        public virtual void RedirectContext(HttpContextBase httpContext, HttpRedirectRuleMatch ruleMatch)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext", "httpContext cannot be null.");
            }

            if (ruleMatch == null)
            {
                throw new ArgumentNullException("ruleMatch", "rule cannot be null.");
            }

            if (!String.IsNullOrEmpty(ruleMatch.RedirectResult))
            {
                httpContext.Response.Clear();
                httpContext.Response.StatusCode = ruleMatch.Rule.RedirectType == HttpRedirectType.Permanent ? 301 : 302;
                httpContext.Response.AddHeader("Location", ruleMatch.RedirectResult);
                httpContext.Response.End();
            }
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Raises this module's <see cref="HttpApplication"/>'s BeginRequest event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ContextBeginRequest(object sender, EventArgs e)
        {
            HttpContextBase httpContext = new HttpContextWrapper(((HttpApplication)sender).Context);
            HttpRedirectRuleMatch ruleMatch = this.GetMatchingRule(httpContext);

            if (ruleMatch != null)
            {
                this.RedirectContext(httpContext, ruleMatch);
            }
        }

        #endregion
    }
}