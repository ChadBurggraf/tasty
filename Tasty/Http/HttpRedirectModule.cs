using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;

namespace Tasty.Http
{
    /// <summary>
    /// Implements <see cref="IHttpModule"/> to do simple regular-expression based HTTP redirection.
    /// </summary>
    public class HttpRedirectModule : IHttpModule
    {
        #region Member Variables

        private const string MATCH_CACHE_KEY = "Tasty.Http.HttpRedirectModule.Cache";

        #endregion

        #region Construction

        /// <summary>
        /// Constructor.
        /// </summary>
        static HttpRedirectModule()
        {
            RuleMatcher = new HttpRedirectRuleMatcher();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the rule matcher used when matching rules.
        /// </summary>
        public static HttpRedirectRuleMatcher RuleMatcher { get; private set; }

        #endregion

        #region Instance Methods

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
                Dictionary<string, HttpRedirectRuleMatch> cache = (Dictionary<string, HttpRedirectRuleMatch>)(httpContext.Cache[MATCH_CACHE_KEY] ?? new Dictionary<string, HttpRedirectRuleMatch>());
                string key = httpContext.Request.Url.AbsoluteUri.ToUpperInvariant();
                bool cacheUpdated = false;

                if (cache.ContainsKey(key))
                {
                    ruleMatch = cache[key];
                }
                else
                {
                    ruleMatch = RuleMatcher.Match(httpContext.Request.Url, HttpSettings.Section.Redirects);
                    cache[key] = ruleMatch;
                    cacheUpdated = true;
                }

                if (cacheUpdated)
                {
                    httpContext.Cache.Remove(MATCH_CACHE_KEY);
                    httpContext.Cache.Add(
                        MATCH_CACHE_KEY,
                        cache,
                        null,
                        DateTime.Now.AddMinutes(15),
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.Normal,
                        null
                    );
                }
            }

            return ruleMatch;
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

        #region IHttpModule Members

        /// <summary>
        /// Disposes of any unmanaged resources.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Initializes the module.
        /// </summary>
        /// <param name="context">The <see cref="HttpApplication"/> that is handling the current request.</param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Raises this module's <see cref="HttpApplication"/>'s BeginRequest event.
        /// </summary>
        private void context_BeginRequest(object sender, EventArgs e)
        {
            HttpContextBase httpContext = new HttpContextWrapper(((HttpApplication)sender).Context);
            HttpRedirectRuleMatch ruleMatch = GetMatchingRule(httpContext);

            if (ruleMatch != null)
            {
                RedirectContext(httpContext, ruleMatch);
            }
        }

        #endregion
    }
}
