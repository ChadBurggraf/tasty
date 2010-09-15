//-----------------------------------------------------------------------
// <copyright file="UrlTokenUrlProvider.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web.UrlTokens
{
    using System;

    /// <summary>
    /// Base <see cref="IUrlTokenUrlProvider{TToken}"/> implementation.
    /// </summary>
    /// <typeparam name="TToken">The <see cref="IUrlToken"/> type to provide URLs for.</typeparam>
    public abstract class UrlTokenUrlProvider<TToken> : IUrlTokenUrlProvider<TToken>
        where TToken : IUrlToken
    {
        /// <summary>
        /// Parses a <see cref="Uri"/> into a <see cref="IUrlToken"/> using the <see cref="UrlTokenStore.Current"/>
        /// <see cref="IUrlTokenStore"/>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to parse.</param>
        /// <returns>The <see cref="IUrlToken"/> identified by the given <see cref="Uri"/>.</returns>
        public TToken TokenFromUrl(Uri uri)
        {
            return this.TokenFromUrl(uri, UrlTokenStore.Current);
        }

        /// <summary>
        /// Parses a <see cref="Uri"/> into a <see cref="IUrlToken"/>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to parse.</param>
        /// <param name="tokenStore">The <see cref="IUrlTokenStore"/> to use when loading token data.</param>
        /// <returns>The <see cref="IUrlToken"/> identified by the given <see cref="Uri"/>.</returns>
        public abstract TToken TokenFromUrl(Uri uri, IUrlTokenStore tokenStore);

        /// <summary>
        /// Generates a <see cref="Uri"/> from the given <see cref="IUrlToken"/> using the <see cref="UrlTokenStore.Current"/>
        /// <see cref="IUrlTokenStore"/>.
        /// </summary>
        /// <param name="token">The <see cref="IUrlToken"/> to generate the <see cref="Uri"/> from.</param>
        /// <returns>The generated <see cref="Uri"/>.</returns>
        public Uri UrlFromToken(TToken token)
        {
            return this.UrlFromToken(token, UrlTokenStore.Current);
        }

        /// <summary>
        /// Generates a <see cref="Uri"/> from the given <see cref="IUrlToken"/>.
        /// </summary>
        /// <param name="token">The <see cref="IUrlToken"/> to generate the <see cref="Uri"/> from.</param>
        /// <param name="tokenStore">The <see cref="IUrlTokenStore"/> to use when saving token data.</param>
        /// <returns>The generated <see cref="Uri"/>.</returns>
        public abstract Uri UrlFromToken(TToken token, IUrlTokenStore tokenStore);
    }
}
