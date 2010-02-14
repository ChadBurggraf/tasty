//-----------------------------------------------------------------------
// <copyright file="IUrlTokenStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web.UrlTokens
{
    using System;

    /// <summary>
    /// Interface definition for persistent URL token stores.
    /// </summary>
    public interface IUrlTokenStore
    {
        /// <summary>
        /// Cleans all expired token records from the store.
        /// </summary>
        void CleanExpiredUrlTokens();

        /// <summary>
        /// Creates a new URL token record.
        /// </summary>
        /// <param name="record">The URL token record to create.</param>
        void CreateUrlToken(UrlTokenRecord record);

        /// <summary>
        /// Gets a URL token record.
        /// </summary>
        /// <param name="key">The key of the record to get.</param>
        /// <returns>The URL token record identified by the given key.</returns>
        UrlTokenRecord GetUrlToken(string key);
    }
}
