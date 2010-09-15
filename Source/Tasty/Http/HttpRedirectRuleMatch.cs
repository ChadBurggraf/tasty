//-----------------------------------------------------------------------
// <copyright file="HttpRedirectRuleMatch.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Http
{
    using System;
    using System.Text.RegularExpressions;
    using Tasty.Configuration;
    
    /// <summary>
    /// Represents a matching HTTP redirect rule.
    /// </summary>
    public class HttpRedirectRuleMatch
    {
        #region Private Fields

        private string redirectResult;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the HttpRedirectRuleMatch class.
        /// </summary>
        /// <param name="rule">The rule that matched.</param>
        /// <param name="matchResult">The result of the match.</param>
        public HttpRedirectRuleMatch(HttpRedirectRuleElement rule, Match matchResult)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule", "rule cannot be null.");
            }

            if (matchResult == null)
            {
                throw new ArgumentNullException("matchResult", "matchResult cannot be null.");
            }

            this.Rule = rule;
            this.MatchResult = matchResult;
        }

        #endregion

        #region Public Instance Properties

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
                if (this.redirectResult == null)
                {
                    this.redirectResult = this.MatchResult.Success ? this.MatchResult.Result(this.Rule.RedirectsTo) : String.Empty;
                }

                return this.redirectResult;
            }
        }

        /// <summary>
        /// Gets the rule that matched.
        /// </summary>
        public HttpRedirectRuleElement Rule { get; private set; }

        #endregion
    }
}