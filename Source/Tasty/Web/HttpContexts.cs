//-----------------------------------------------------------------------
// <copyright file="HttpContexts.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web
{
    using System;
    using System.Web;

    /// <summary>
    /// Provides extensions to <see cref="HttpContext"/> and <see cref="HttpContextBase"/>.
    /// </summary>
    public static class HttpContexts
    {
        /// <summary>
        /// Resolves a URL string using the given HTTP context. The URL can be absolute (e.g., /some/path) or
        /// application-relative (e.g., ~/some/path).
        /// </summary>
        /// <param name="httpContext">The HTTP context to use when resolving the URL.</param>
        /// <param name="url">The URL to resolve.</param>
        /// <returns>A resolved URL string.</returns>
        public static string ResolveUrl(this HttpContext httpContext, string url)
        {
            return ResolveUrl(httpContext, url, false);
        }

        /// <summary>
        /// Resolves a URL string using the given HTTP context. The URL can be absolute (e.g., /some/path) or
        /// application-relative (e.g., ~/some/path).
        /// </summary>
        /// <param name="httpContext">The HTTP context to use when resolving the URL.</param>
        /// <param name="url">The URL to resolve.</param>
        /// <param name="fullyQualify">A value indicating whether to fully qualify the URL.</param>
        /// <returns>A resolved URL string.</returns>
        public static string ResolveUrl(this HttpContext httpContext, string url, bool fullyQualify)
        {
            return ResolveUrl(httpContext, url, fullyQualify, false);
        }

        /// <summary>
        /// Resolves a URL string using the given HTTP context. The URL can be absolute (e.g., /some/path) or
        /// application-relative (e.g., ~/some/path).
        /// </summary>
        /// <param name="httpContext">The HTTP context to use when resolving the URL.</param>
        /// <param name="url">The URL to resolve.</param>
        /// <param name="fullyQualify">A value indicating whether to fully qualify the URL.</param>
        /// <param name="forceSsl">A value indicating whether to force the resolved URL to use SSL, even if
        /// the HTTP context's request does not. Leave false to use the same scheme as the HTTP context's request.</param>
        /// <returns>A resolved URL string.</returns>
        public static string ResolveUrl(this HttpContext httpContext, string url, bool fullyQualify, bool forceSsl)
        {
            return ResolveUrl(new HttpContextWrapper(httpContext), url, fullyQualify, forceSsl);
        }

        /// <summary>
        /// Resolves a URL string using the given HTTP context. The URL can be absolute (e.g., /some/path) or
        /// application-relative (e.g., ~/some/path).
        /// </summary>
        /// <param name="httpContext">The HTTP context to use when resolving the URL.</param>
        /// <param name="url">The URL to resolve.</param>
        /// <returns>A resolved URL string.</returns>
        public static string ResolveUrl(this HttpContextBase httpContext, string url)
        {
            return ResolveUrl(httpContext, url, false);
        }

        /// <summary>
        /// Resolves a URL string using the given HTTP context. The URL can be absolute (e.g., /some/path) or
        /// application-relative (e.g., ~/some/path).
        /// </summary>
        /// <param name="httpContext">The HTTP context to use when resolving the URL.</param>
        /// <param name="url">The URL to resolve.</param>
        /// <param name="fullyQualify">A value indicating whether to fully qualify the URL.</param>
        /// <returns>A resolved URL string.</returns>
        public static string ResolveUrl(this HttpContextBase httpContext, string url, bool fullyQualify)
        {
            return ResolveUrl(httpContext, url, fullyQualify, false);
        }

        /// <summary>
        /// Resolves a URL string using the given HTTP context. The URL can be absolute (e.g., /some/path) or
        /// application-relative (e.g., ~/some/path).
        /// </summary>
        /// <param name="httpContext">The HTTP context to use when resolving the URL.</param>
        /// <param name="url">The URL to resolve.</param>
        /// <param name="fullyQualify">A value indicating whether to fully qualify the URL.</param>
        /// <param name="forceSsl">A value indicating whether to force the resolved URL to use SSL, even if
        /// the HTTP context's request does not. Leave false to use the same scheme as the HTTP context's request.</param>
        /// <returns>A resolved URL string.</returns>
        public static string ResolveUrl(this HttpContextBase httpContext, string url, bool fullyQualify, bool forceSsl)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext", "httpContext cannot be null.");
            }

            if (String.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url", "url must contain a value.");
            }

            if (url.StartsWith(".", StringComparison.Ordinal))
            {
                throw new ArgumentException("url cannot start with a period (.) or double period (..).", "url");
            }

            if (!IsFullyQualifiedUrl(url))
            {
                if (url.StartsWith("~", StringComparison.Ordinal))
                {
                    url = Uris.Combine(httpContext.Request.ApplicationPath, url.Substring(1));
                }

                if (fullyQualify)
                {
                    Uri uri = new Uri(httpContext.Request.Url, url);
                    url = uri.ToString();
                    forceSsl = forceSsl || httpContext.Request.Url.Scheme == Uri.UriSchemeHttps;
                }
            }

            if (forceSsl)
            {
                UriBuilder builder = new UriBuilder(url);
                builder.Scheme = Uri.UriSchemeHttps;
                builder.Port = 443;
                url = builder.Uri.ToString();
            }

            return url;
        }

        /// <summary>
        /// Gets a value indicating whether the given URL string is a fully qualified.
        /// </summary>
        /// <param name="url">The URL to check.</param>
        /// <returns>True if hte URL is fully qualified, false otherwise.</returns>
        public static bool IsFullyQualifiedUrl(string url)
        {
            bool isAbsolute = false;

            if (!String.IsNullOrEmpty(url))
            {
                int separator = url.IndexOf("://", StringComparison.Ordinal);

                if (separator > -1) 
                {
                    int query = url.IndexOf("?", StringComparison.Ordinal);
                    isAbsolute = query < 0 || separator < query;
                }
            }

            return isAbsolute;
        }
    }
}
