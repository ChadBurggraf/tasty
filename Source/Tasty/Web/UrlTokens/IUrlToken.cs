//-----------------------------------------------------------------------
// <copyright file="IUrlToken.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web.UrlTokens
{
    using System;

    /// <summary>
    /// Defines the interface for URL tokens.
    /// </summary>
    public interface IUrlToken
    {
        /// <summary>
        /// Gets or sets a value indicating when the token expires, in UTC.
        /// </summary>
        DateTime Expires { get; set; }

        /// <summary>
        /// Gets the number of hours from creation the URL token expires in.
        /// </summary>
        int ExpiryHours { get; }

        /// <summary>
        /// Gets a value indicating whether the token is expired.
        /// </summary>
        bool IsExpired { get; }

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
    }
}
