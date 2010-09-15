//-----------------------------------------------------------------------
// <copyright file="NameValueCollections.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;

    /// <summary>
    /// Provides extensions and helpers for <see cref="System.Collections.Specialized.NameValueCollection"/>s.
    /// </summary>
    public static class NameValueCollections
    {
        /// <summary>
        /// Clears and then fills the collection with the key/value pairs in the given <see cref="KeyValueConfigurationCollection"/>.
        /// </summary>
        /// <param name="collection">The collection to fill.</param>
        /// <param name="configCollection">The <see cref="KeyValueConfigurationCollection"/> to use as a fill source.</param>
        public static void FillWith(this NameValueCollection collection, KeyValueConfigurationCollection configCollection)
        {
            collection.Clear();

            foreach (KeyValueConfigurationElement element in configCollection)
            {
                collection.Add(element.Key, element.Value);
            }
        }

        /// <summary>
        /// Adds the given key/value pair to a copy of the collection and then returns the new collection.
        /// </summary>
        /// <param name="collection">The collection to add the key/value pair to.</param>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        /// <returns>The modified collection.</returns>
        public static NameValueCollection With(this NameValueCollection collection, string key, string value)
        {
            NameValueCollection copy = new NameValueCollection(collection);
            copy.Add(key, value);

            return copy;
        }

        /// <summary>
        /// Removes the given key from a copy of the collection and then returns the new collection.
        /// </summary>
        /// <param name="collection">The collection to remove the key from.</param>
        /// <param name="key">The key to remove.</param>
        /// <returns>The modified collection.</returns>
        public static NameValueCollection Without(this NameValueCollection collection, string key)
        {
            NameValueCollection copy = new NameValueCollection(collection);
            copy.Remove(key);

            return copy;
        }
    }
}