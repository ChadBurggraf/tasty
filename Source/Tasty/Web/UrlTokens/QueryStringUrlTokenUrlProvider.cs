//-----------------------------------------------------------------------
// <copyright file="QueryStringUrlTokenUrlProvider.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web.UrlTokens
{
    using System;

    /// <summary>
    /// Extends <see cref="UrlTokenUrlProvider{TToken}"/> for URL tokens in the query string.
    /// </summary>
    /// <typeparam name="TToken">The <see cref="IUrlToken"/> type to provide URLs for.</typeparam>
    public class QueryStringUrlTokenUrlProvider<TToken> : UrlTokenUrlProvider<TToken>
        where TToken : IUrlToken
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
        /// Parses a <see cref="Uri"/> into a <see cref="IUrlToken"/>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to parse.</param>
        /// <param name="tokenStore">The <see cref="IUrlTokenStore"/> to use when loading token data.</param>
        /// <returns>The <see cref="IUrlToken"/> identified by the given <see cref="Uri"/>.</returns>
        public override TToken TokenFromUrl(Uri uri, IUrlTokenStore tokenStore)
        {
            if (String.IsNullOrEmpty(this.QueryStringKey))
            {
                throw new InvalidOperationException("QueryStringKey must be set to a value.");
            }

            if (uri == null)
            {
                throw new ArgumentNullException("uri", "uri cannot be null.");
            }

            TToken token = default(TToken);
            string key = QueryString.FromUrl(uri)[this.QueryStringKey];

            if (!String.IsNullOrEmpty(key))
            {
                UrlTokenRecord record = tokenStore.GetUrlToken(key);

                if (record != null)
                {
                    token = (TToken)record.ToUrlToken();
                }
            }

            return token;
        }

        /// <summary>
        /// Generates a <see cref="Uri"/> from the given <see cref="IUrlToken"/>.
        /// </summary>
        /// <param name="token">The <see cref="IUrlToken"/> to generate the <see cref="Uri"/> from.</param>
        /// <param name="tokenStore">The <see cref="IUrlTokenStore"/> to use when saving token data.</param>
        /// <returns>The generated <see cref="Uri"/>.</returns>
        public override Uri UrlFromToken(TToken token, IUrlTokenStore tokenStore)
        {
            if (String.IsNullOrEmpty(this.QueryStringKey))
            {
                throw new InvalidOperationException("QueryStringKey must be set to a value.");
            }

            if (this.Url == null)
            {
                throw new InvalidOperationException("Url must be set to a value.");
            }

            string key = token.GenerateKey();

            tokenStore.CreateUrlToken(new UrlTokenRecord()
            {
                Created = DateTime.UtcNow,
                Data = token.Serialize(),
                Expires = DateTime.UtcNow.AddHours(token.ExpiryHours),
                Key = key,
                TokenType = token.GetType()
            });

            return this.Url.SetQueryValue(this.QueryStringKey, key);
        }
    }
}
