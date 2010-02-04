//-----------------------------------------------------------------------
// <copyright file="NameValueCollections.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Collections.Specialized;

    /// <summary>
    /// Provides extensions and helpers for <see cref="System.Collections.Specialized.NameValueCollection"/>s.
    /// </summary>
    public static class NameValueCollections
    {
        /// <summary>
        /// Adds the given key/value pair to the collection and then returns the modified collection.
        /// </summary>
        /// <param name="collection">The collection to add the key/value pair to.</param>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        /// <returns>The modified collection.</returns>
        public static NameValueCollection With(this NameValueCollection collection, string key, string value)
        {
            collection.Add(key, value);
            return collection;
        }

        /// <summary>
        /// Removes the given key from the collection and then returns the modified collection.
        /// </summary>
        /// <param name="collection">The collection to remove the key from.</param>
        /// <param name="key">The key to remove.</param>
        /// <returns>The modified collection.</returns>
        public static NameValueCollection Without(this NameValueCollection collection, string key)
        {
            collection.Remove(key);
            return collection;
        }
    }
}