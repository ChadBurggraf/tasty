//-----------------------------------------------------------------------
// <copyright file="QueryString.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Text;
    using System.Web;

    /// <summary>
    /// Represents a URL query string as a key/value collection.
    /// </summary>
    public class QueryString
    {
        #region Private Fields

        private NameValueCollection innerCollection;
        private ReadOnlyCollection<string> keys;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the QueryString class.
        /// </summary>
        public QueryString()
        {
            this.innerCollection = new NameValueCollection();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a collection of all of the keys in the query string.
        /// </summary>
        public ReadOnlyCollection<string> Keys
        {
            get
            {
                if (this.keys == null)
                {
                    this.keys = new ReadOnlyCollection<string>(this.innerCollection.AllKeys);
                }

                return this.keys;
            }
        }

        /// <summary>
        /// Gets or sets the value for the specified key.
        /// </summary>
        /// <param name="key">The key to get or set the value for.</param>
        /// <returns>The key's value.</returns>
        public string this[string key]
        {
            get
            {
                return this.Get(key);
            }

            set
            {
                this.Set(key, value);
            }
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Parses a <see cref="QueryString"/> from the given <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to the query of.</param>
        /// <returns>The parsed <see cref="QueryString"/> object.</returns>
        public static QueryString FromUrl(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri", "uri must have a value.");
            }

            return Parse(uri.Query);
        }

        /// <summary>
        /// Parses the given query string into a <see cref="QueryString"/> instance.
        /// </summary>
        /// <param name="query">The query string to parse.</param>
        /// <returns>The parsed <see cref="QueryString"/> object.</returns>
        public static QueryString Parse(string query)
        {
            QueryString collection = new QueryString();

            if (!String.IsNullOrEmpty(query))
            {
                if (query.StartsWith("?", StringComparison.Ordinal))
                {
                    query = query.Substring(1);
                }

                string[] parts = query.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string part in parts)
                {
                    string[] pair = part.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                    if (!String.IsNullOrEmpty(pair[0]))
                    {
                        string key = HttpUtility.UrlDecode(pair[0]);

                        if (pair.Length > 1)
                        {
                            string[] values = HttpUtility.UrlDecode(pair[1]).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (string value in values)
                            {
                                collection.Add(key, value);
                            }
                        }
                    }
                }
            }

            return collection;
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Adds a value to the query string for the specified key.
        /// </summary>
        /// <param name="key">The key to add the value for.</param>
        /// <param name="value">The value to add.</param>
        public void Add(string key, string value)
        {
            this.innerCollection.Add(key, value);
            this.keys = null;
        }

        /// <summary>
        /// Gets the value for the specified key.
        /// </summary>
        /// <param name="key">The key to get the value for.</param>
        /// <returns>The key's value.</returns>
        public string Get(string key)
        {
            return this.innerCollection[key];
        }

        /// <summary>
        /// Removes the specified key and its value(s) from the query string.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        public void Remove(string key)
        {
            this.innerCollection.Remove(key);
            this.keys = null;
        }

        /// <summary>
        /// Sets the value in the query string for the specified key.
        /// </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public void Set(string key, string value)
        {
            this.innerCollection[key] = value;
            this.keys = null;
        }

        /// <summary>
        /// Converts this instance to a URL-encoded query string.
        /// </summary>
        /// <returns>A URL-encoded query string.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < this.innerCollection.Count; i++)
            {
                string key = HttpUtility.UrlEncode(this.innerCollection.GetKey(i));

                foreach (string value in this.innerCollection.GetValues(i))
                {
                    sb.Append(key);
                    sb.Append("=");
                    sb.Append(HttpUtility.UrlEncode(value));
                    sb.Append("&");
                }
            }

            if (sb.Length > 0 && sb[sb.Length - 1] == '&')
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }

        #endregion
    }
}
