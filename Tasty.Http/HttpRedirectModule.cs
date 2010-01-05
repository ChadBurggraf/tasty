﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

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

        #region Fields

        /// <summary>
        /// Gets the rule matcher used when matching rules.
        /// </summary>
        public static readonly HttpRedirectRuleMatcher RuleMatcher = new HttpRedirectRuleMatcher();

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
                throw new ArgumentNullException("httpContext cannot be null.", "httpContext");
            }

            HttpRedirectRuleMatch ruleMatch = null;

            lock (httpContext.Cache)
            {
                Dictionary<string, HttpRedirectRuleMatch> cache = (Dictionary<string, HttpRedirectRuleMatch>)(httpContext.Cache[MATCH_CACHE_KEY] ?? new Dictionary<string, HttpRedirectRuleMatch>());
                string key = httpContext.Request.Url.AbsoluteUri.ToUpperInvariant();

                if (cache.ContainsKey(key))
                {
                    ruleMatch = cache[key];
                }
                else
                {
                    ruleMatch = RuleMatcher.Match(httpContext.Request.Url, HttpSettings.Section.Redirects);
                    cache[key] = ruleMatch;
                }

                httpContext.Cache[MATCH_CACHE_KEY] = cache;
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
                throw new ArgumentNullException("httpContext cannot be null.", "httpContext");
            }

            if (ruleMatch == null)
            {
                throw new ArgumentNullException("rule cannot be null.", "rule");
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
