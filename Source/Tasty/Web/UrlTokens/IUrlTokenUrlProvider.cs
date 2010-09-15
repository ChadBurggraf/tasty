//-----------------------------------------------------------------------
// <copyright file="IUrlTokenUrlProvider.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web.UrlTokens
{
    using System;

    /// <summary>
    /// Interface definition for URL providers for <see cref="IUrlToken"/>s.
    /// </summary>
    /// <typeparam name="TToken">The type of <see cref="IUrlToken"/> to provide URLs for.</typeparam>
    public interface IUrlTokenUrlProvider<TToken>
        where TToken : IUrlToken
    {
        /// <summary>
        /// Parses a <see cref="Uri"/> into a <see cref="IUrlToken"/> using the <see cref="UrlTokenStore.Current"/>
        /// <see cref="IUrlTokenStore"/>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to parse.</param>
        /// <returns>The <see cref="IUrlToken"/> identified by the given <see cref="Uri"/>.</returns>
        TToken TokenFromUrl(Uri uri);

        /// <summary>
        /// Parses a <see cref="Uri"/> into a <see cref="IUrlToken"/>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to parse.</param>
        /// <param name="tokenStore">The <see cref="IUrlTokenStore"/> to use when loading token data.</param>
        /// <returns>The <see cref="IUrlToken"/> identified by the given <see cref="Uri"/>.</returns>
        TToken TokenFromUrl(Uri uri, IUrlTokenStore tokenStore);

        /// <summary>
        /// Generates a <see cref="Uri"/> from the given <see cref="IUrlToken"/> using the <see cref="UrlTokenStore.Current"/>
        /// <see cref="IUrlTokenStore"/>.
        /// </summary>
        /// <param name="token">The <see cref="IUrlToken"/> to generate the <see cref="Uri"/> from.</param>
        /// <returns>The generated <see cref="Uri"/>.</returns>
        Uri UrlFromToken(TToken token);

        /// <summary>
        /// Generates a <see cref="Uri"/> from the given <see cref="IUrlToken"/>.
        /// </summary>
        /// <param name="token">The <see cref="IUrlToken"/> to generate the <see cref="Uri"/> from.</param>
        /// <param name="tokenStore">The <see cref="IUrlTokenStore"/> to use when saving token data.</param>
        /// <returns>The generated <see cref="Uri"/>.</returns>
        Uri UrlFromToken(TToken token, IUrlTokenStore tokenStore);
    }
}
