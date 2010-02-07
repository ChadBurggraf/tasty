//-----------------------------------------------------------------------
// <copyright file="IUrlToken.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web
{
    using System;

    /// <summary>
    /// Defines the interface for URL tokens.
    /// </summary>
    public interface IUrlToken
    {
        /// <summary>
        /// Gets the number of hours from creation the URL token expires in.
        /// </summary>
        int ExpiryHours { get; }

        /// <summary>
        /// Generates a new unique key that can be used to identify the URL token.
        /// </summary>
        /// <returns>A unique token identifier.</returns>
        string GenerateKey();

        /// <summary>
        /// Serializes the URL token for storage.
        /// </summary>
        /// <returns>The serialized URL token data.</returns>
        string Serialize();

        /// <summary>
        /// Creates a URL that identifies this instance by persisting
        /// this instance to the currently configured <see cref="IUrlTokenStore"/> and
        /// using the given <see cref="IUrlTokenUrlProvider"/> to generate a <see cref="Uri"/>.
        /// </summary>
        /// <param name="urlProvider">The <see cref="IUrlTokenUrlProvider"/> to use when generating the <see cref="Uri"/>.</param>
        /// <returns>A <see cref="Uri"/> that identifies this instance.</returns>
        Uri ToUrl(IUrlTokenUrlProvider urlProvider);

        /// <summary>
        /// Creates a URL that identifies this instance by persisting
        /// this instance to the given <see cref="IUrlTokenStore"/> and
        /// using the given <see cref="IUrlTokenUrlProvider"/> to generate
        /// a <see cref="Uri"/>.
        /// </summary>
        /// <param name="urlProvider">The <see cref="IUrlTokenUrlProvider"/> to use when generating the <see cref="Uri"/>.</param>
        /// <param name="tokenStore">The <see cref="IUrlTokenStore"/> to use when persisting the token's data.</param>
        /// <returns>A <see cref="Uri"/> that identifies this instance.</returns>
        Uri ToUrl(IUrlTokenUrlProvider urlProvider, IUrlTokenStore tokenStore);
    }
}
