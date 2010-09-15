//-----------------------------------------------------------------------
// <copyright file="Uris.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web
{
    using System;

    /// <summary>
    /// Providest extensions and helpers for URLs.
    /// </summary>
    public static class Uris
    {
        /// <summary>
        /// Adds the given key/value to the given URL's query string.
        /// </summary>
        /// <param name="url">The URL to add the query string value to.</param>
        /// <param name="key">The query string key to add.</param>
        /// <param name="value">The query string value to add.</param>
        /// <returns>The updated URL.</returns>
        public static Uri AddQueryValue(this Uri url, string key, string value)
        {
            UriBuilder builder = new UriBuilder(url);

            QueryString query = QueryString.Parse(url.Query);
            query.Add(key, value);

            builder.Query = query.ToString();

            return builder.Uri;
        }

        /// <summary>
        /// Appens the given path to the URI's path.
        /// </summary>
        /// <param name="uri">The URI to append the path to.</param>
        /// <param name="path">The path to append.</param>
        /// <returns>A URI with the path appended.</returns>
        public static Uri AppendPath(this Uri uri, string path)
        {
            UriBuilder builder = new UriBuilder(uri);
            builder.Path = Combine(builder.Path, path);

            return builder.Uri;
        }

        /// <summary>
        /// Clears the given URI's query string.
        /// </summary>
        /// <param name="uri">The URI to clear the question string from.</param>
        /// <returns>The URI with its query string cleared.</returns>
        public static Uri ClearQueryString(this Uri uri)
        {
            UriBuilder builder = new UriBuilder(uri);
            builder.Query = String.Empty;

            return builder.Uri;
        }

        /// <summary>
        /// Combines the two URL parts with a URL path separator.
        /// </summary>
        /// <param name="first">The first part to combine.</param>
        /// <param name="second">The second part to combine.</param>
        /// <returns>The combined URL.</returns>
        public static string Combine(string first, string second)
        {
            first = (first ?? String.Empty).Trim();
            second = (second ?? String.Empty).Trim();

            if (first.EndsWith("/", StringComparison.Ordinal))
            {
                first = first.Substring(0, first.Length - 1);
            }

            if (second.StartsWith("/", StringComparison.Ordinal))
            {
                second = second.Substring(1);
            }

            if (!String.IsNullOrEmpty(first) && !String.IsNullOrEmpty(second))
            {
                return String.Concat(first, "/", second);
            }
            else if (!String.IsNullOrEmpty(first))
            {
                return first;
            }
            else
            {
                return second;
            }
        }

        /// <summary>
        /// Gets the file name part of the URI.
        /// </summary>
        /// <param name="uri">The URI to get the file name part of.</param>
        /// <returns>The file name part of the URI.</returns>
        public static string FileName(this Uri uri)
        {
            int lastSlash = uri.AbsolutePath.LastIndexOf("/", StringComparison.Ordinal);

            if (lastSlash >= 0 && lastSlash < uri.AbsolutePath.Length - 1)
            {
                return uri.AbsolutePath.Substring(lastSlash + 1);
            }

            return String.Empty;
        }

        /// <summary>
        /// Gets the value of the given key in the given URL's query string.
        /// </summary>
        /// <param name="url">The URL to get the query string value from.</param>
        /// <param name="key">The query string key to get the value for.</param>
        /// <returns>The query string value.</returns>
        public static string QueryValue(this Uri url, string key)
        {
            return QueryString.Parse(url.Query)[key];
        }

        /// <summary>
        /// Removes the given key from the given URL's query string.
        /// </summary>
        /// <param name="url">The URL to remove the query string key from.</param>
        /// <param name="key">The query string key to remove.</param>
        /// <returns>The updated URL.</returns>
        public static Uri RemoveQueryValue(this Uri url, string key)
        {
            UriBuilder builder = new UriBuilder(url);

            QueryString query = QueryString.Parse(url.Query);
            query.Remove(key);

            builder.Query = query.ToString();

            return builder.Uri;
        }

        /// <summary>
        /// Sets the given path value for the given URL.
        /// </summary>
        /// <param name="url">The URL to set the path for.</param>
        /// <param name="path">The path to set.</param>
        /// <returns>The updated URL.</returns>
        public static Uri SetPath(this Uri url, string path)
        {
            path = path ?? String.Empty;

            if (path.StartsWith("/", StringComparison.Ordinal))
            {
                path = path.Substring(1);
            }

            UriBuilder builder = new UriBuilder(url);
            builder.Path = path;

            return builder.Uri;
        }

        /// <summary>
        /// Sets the entire query string for the given URL.
        /// </summary>
        /// <param name="url">The URL to set the query string for.</param>
        /// <param name="query">The query string to set.</param>
        /// <returns>The updated URL.</returns>
        public static Uri SetQuery(this Uri url, string query)
        {
            return SetQuery(url, QueryString.Parse(query ?? String.Empty));
        }

        /// <summary>
        /// Sets the entire query string for the given URL.
        /// </summary>
        /// <param name="url">The URl to set the query string for.</param>
        /// <param name="query">The query string to set.</param>
        /// <returns>The updated URL.</returns>
        public static Uri SetQuery(this Uri url, QueryString query)
        {
            UriBuilder builder = new UriBuilder(url);
            builder.Query = query != null ? query.ToString() : String.Empty;

            return builder.Uri;
        }

        /// <summary>
        /// Sets the given key/value for the given URL's query string.
        /// </summary>
        /// <param name="url">The URL to set the query string value for.</param>
        /// <param name="key">The query string key to set.</param>
        /// <param name="value">The query string value to set.</param>
        /// <returns>The updated URL.</returns>
        public static Uri SetQueryValue(this Uri url, string key, string value)
        {
            UriBuilder builder = new UriBuilder(url);

            QueryString query = QueryString.Parse(url.Query);
            query.Set(key, value);

            builder.Query = query.ToString();

            return builder.Uri;
        }
    }
}
