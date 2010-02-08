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
        /// Separates a <see cref="IUrlToken"/> key out of the given URL.
        /// Should return null or empty if no valid key could be found.
        /// </summary>
        /// <param name="uri">The URL to separate the key from.</param>
        /// <returns>The <see cref="IUrlToken"/> key, or null or empty if none was found.</returns>
        string SeparateKey(Uri uri);

        /// <summary>
        /// Creates a new <see cref="Uri"/> with the given <see cref="IUrlToken"/> key embedded 
        /// (e.g., as a query string parameter).
        /// </summary>
        /// <param name="key">The <see cref="IUrlToken"/> key to create the <see cref="Uri"/> with.</param>
        /// <returns>A <see cref="Uri"/> with the given <see cref="IUrlToken"/> key embedded.</returns>
        Uri UrlWithKey(string key);
    }
}
