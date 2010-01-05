using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Tasty.Http
{
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
                throw new ArgumentNullException("requestUrl cannot be null.", "requestUri");
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
