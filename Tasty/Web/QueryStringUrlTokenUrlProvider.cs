//-----------------------------------------------------------------------
// <copyright file="QueryStringUrlTokenUrlProvider.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web
{
    using System;

    /// <summary>
    /// Implements <see cref="IUrlTokenUrlProvider"/> for URL tokens in the query string.
    /// </summary>
    public class QueryStringUrlTokenUrlProvider : IUrlTokenUrlProvider
    {
        /// <summary>
        /// Gets or sets the key to use for the URL token in the query string.
        /// </summary>
        public string QueryStringKey { get; set; }

        /// <summary>
        /// Gets or sets the URL to use when generating a URL with a URL token.
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        /// Separates a <see cref="IUrlToken"/> key out of the given URL.
        /// Should return null or empty if no valid key could be found.
        /// </summary>
        /// <param name="uri">The URL to separate the key from.</param>
        /// <returns>The <see cref="IUrlToken"/> key, or null or empty if none was found.</returns>
        public string SeparateKey(Uri uri)
        {
            if (String.IsNullOrEmpty(this.QueryStringKey))
            {
                throw new InvalidOperationException("QueryStringKey must be set to a value.");
            }

            if (uri == null)
            {
                throw new ArgumentNullException("uri", "uri cannot be null.");
            }

            return QueryString.FromUrl(uri)[this.QueryStringKey];
        }

        /// <summary>
        /// Creates a new <see cref="Uri"/> with the given <see cref="IUrlToken"/> key embedded 
        /// (e.g., as a query string parameter).
        /// </summary>
        /// <param name="key">The <see cref="IUrlToken"/> key to create the <see cref="Uri"/> with.</param>
        /// <returns>A <see cref="Uri"/> with the given <see cref="IUrlToken"/> key embedded.</returns>
        public Uri UrlWithKey(string key)
        {
            if (String.IsNullOrEmpty(this.QueryStringKey))
            {
                throw new InvalidOperationException("QueryStringKey must be set to a value.");
            }

            if (this.Url == null)
            {
                throw new InvalidOperationException("Url must be set to a value.");
            }

            return this.Url.SetQueryValue(this.QueryStringKey, key);
        }
    }
}
