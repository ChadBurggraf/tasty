//-----------------------------------------------------------------------
// <copyright file="HttpRedirectRuleMatcher.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Http
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Tasty.Configuration;

    /// <summary>
    /// Matches URIs to <see cref="HttpRedirectRuleElement"/>s.
    /// </summary>
    public class HttpRedirectRuleMatcher
    {
        /// <summary>
        /// Gets a rule matching the given request URI.
        /// </summary>
        /// <param name="requestUri">The request URI to get a matching rule for.</param>
        /// <param name="rules">The rule collection to use when matching.</param>
        /// <returns>A matching rule, or null if none was found.</returns>
        public virtual HttpRedirectRuleMatch Match(Uri requestUri, IEnumerable<HttpRedirectRuleElement> rules)
        {
            if (requestUri == null)
            {
                throw new ArgumentNullException("requestUri", "requestUrl cannot be null.");
            }

            HttpRedirectRuleMatch ruleMatch = null;
            string uriValue = requestUri.ToString();

            if (rules != null)
            {
                foreach (var rule in rules)
                {
                    Match match = Regex.Match(uriValue, rule.Pattern, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        ruleMatch = new HttpRedirectRuleMatch(rule, match);
                        break;
                    }
                }
            }

            return ruleMatch;
        }
    }
}
