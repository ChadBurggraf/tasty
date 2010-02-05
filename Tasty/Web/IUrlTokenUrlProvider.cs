//-----------------------------------------------------------------------
// <copyright file="IUrlTokenUrlProvider.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web
{
    using System;

    /// <summary>
    /// Interface definition for URL providers for <see cref="IUrlToken"/>s.
    /// </summary>
    public interface IUrlTokenUrlProvider
    {
        /// <summary>
        /// Embeds the given <see cref="IUrlToken"/> key into the given URL (e.g., as a query string parameter).
        /// </summary>
        /// <param name="key">The <see cref="IUrlToken"/> key to embed.</param>
        /// <param name="uri">The URL to embed the key into.</param>
        /// <returns>A copy of the given URL with the key embedded.</returns>
        Uri EmbedKey(string key, Uri uri);

        /// <summary>
        /// Separates a <see cref="IUrlToken"/> key out of the given URL.
        /// Should return null or empty if no valid key could be found.
        /// </summary>
        /// <param name="uri">The URL to separate the key from.</param>
        /// <returns>The <see cref="IUrlToken"/> key, or null or empty if none was found.</returns>
        string SeparateKey(Uri uri);
    }
}
