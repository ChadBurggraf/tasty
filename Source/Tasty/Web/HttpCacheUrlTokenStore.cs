//-----------------------------------------------------------------------
// <copyright file="HttpCacheUrlTokenStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Caching;

    /// <summary>
    /// Implements <see cref="IUrlTokenStore"/> to persist <see cref="IUrlToken"/>s
    /// to the current <see cref="HttpRuntime.Cache"/>.
    /// </summary>
    public class HttpCacheUrlTokenStore : IUrlTokenStore
    {
        #region Fields

        private const string CacheKey = "Tasty.Web.HttpCacheUrlTokenStore";
        private static readonly object locker = new object();

        #endregion

        #region Protected Static Properties

        /// <summary>
        /// Gets the storage dictionary from the current <see cref="HttpRuntime.Cache"/>,
        /// creating it if it doesn't exist.
        /// </summary>
        /// <returns>The storage dictionary.</returns>
        protected static IDictionary<string, UrlTokenRecord> Dictionary
        {
            get
            {
                IDictionary<string, UrlTokenRecord> dict = HttpRuntime.Cache[CacheKey] as IDictionary<string, UrlTokenRecord>;

                if (dict == null)
                {
                    dict = new Dictionary<string, UrlTokenRecord>();
                    SaveDictionary(dict);
                }

                return dict;
            }
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Cleans all expired token records from the store.
        /// </summary>
        public virtual void CleanExpiredUrlTokens()
        {
            lock (locker)
            {
                IDictionary<string, UrlTokenRecord> dictionary = Dictionary;

                var expired = (from kv in dictionary
                               where kv.Value.Expires < DateTime.UtcNow
                               select kv).ToArray();

                foreach (var kv in expired)
                {
                    dictionary.Remove(kv);
                }

                SaveDictionary(dictionary);
            }
        }

        /// <summary>
        /// Creates a new URL token record.
        /// </summary>
        /// <param name="record">The URL token record to create.</param>
        public virtual void CreateUrlToken(UrlTokenRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record", "record must have a value.");
            }

            if (String.IsNullOrEmpty(record.Key))
            {
                throw new ArgumentException("record.Key must have a value.", "record");
            }

            lock (locker)
            {
                IDictionary<string, UrlTokenRecord> dict = Dictionary;
                dict[record.Key] = new UrlTokenRecord(record);
                SaveDictionary(dict);
            }

            this.CleanExpiredUrlTokens();
        }

        /// <summary>
        /// Gets a URL token record.
        /// </summary>
        /// <param name="key">The key of the record to get.</param>
        /// <returns>The URL token record identified by the given key.</returns>
        public virtual UrlTokenRecord GetUrlToken(string key)
        {
            lock (locker)
            {
                IDictionary<string, UrlTokenRecord> dict = Dictionary;

                if (dict.ContainsKey(key))
                {
                    return new UrlTokenRecord(dict[key]);
                }
            }

            return null;
        }

        #endregion

        #region Protected Static Methods

        /// <summary>
        /// Saves the updated storage dictionary to the current <see cref="HttpRuntime.Cache"/>.
        /// Before saving, the dictionary is cleaned of any expired records.
        /// </summary>
        /// <param name="dictionary">The dictionary to save.</param>
        protected static void SaveDictionary(IDictionary<string, UrlTokenRecord> dictionary)
        {
            HttpRuntime.Cache.Insert(
                CacheKey,
                dictionary,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                null);
        }

        #endregion
    }
}