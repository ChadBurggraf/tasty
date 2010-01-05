using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Tasty.Http
{
    /// <summary>
    /// Represents a matching HTTP redirect rule.
    /// </summary>
    public class HttpRedirectRuleMatch
    {
        private string redirectResult;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rule">The rule that matched.</param>
        /// <param name="matchResult">The result of the match.</param>
        public HttpRedirectRuleMatch(HttpRedirectRuleElement rule, Match matchResult)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule cannot be null.", "rule");
            }

            if (matchResult == null)
            {
                throw new ArgumentNullException("matchResult cannot be null.", "matchResult");
            }

            Rule = rule;
            MatchResult = matchResult;
        }

        /// <summary>
        /// Gets the result of the match.
        /// </summary>
        public Match MatchResult { get; private set; }

        /// <summary>
        /// Gets the rule's redirect URL transformed by the result of the match.
        /// </summary>
        public string RedirectResult
        {
            get
            {
                if (redirectResult == null)
                {
                    redirectResult = MatchResult.Success ? MatchResult.Result(Rule.RedirectsTo) : String.Empty;
                }

                return redirectResult;
            }
        }

        /// <summary>
        /// Gets the rule that matched.
        /// </summary>
        public HttpRedirectRuleElement Rule { get; private set; }
    }
}
