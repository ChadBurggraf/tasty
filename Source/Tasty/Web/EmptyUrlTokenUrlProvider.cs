using System;

namespace Tasty.Web
{
    /// <summary>
    /// Implements <see cref="IUrlTokenUrlProvider"/> for non-tokenized URLs.
    /// </summary>
    public class EmptyUrlTokenUrlProvider : IUrlTokenUrlProvider
    {
        /// <summary>
        /// Gets or sets the URL to use when generating a URL with a URL token.
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        /// Separates a <see cref="IUrlToken"/> key out of the given URL.
        /// Should return null or empty if no valid key could be found.
        /// </summary>
        /// <param name="uri">The URL to separate the key from.</param>
        /// <returns>The <see cref="IUrlToken"/> key, or null or empty if none was found.</returns>
        public string SeparateKey(Uri uri)
        {
            return String.Empty;
        }

        /// <summary>
        /// Creates a new <see cref="Uri"/> with the given <see cref="IUrlToken"/> key embedded 
        /// (e.g., as a query string parameter).
        /// </summary>
        /// <param name="key">The <see cref="IUrlToken"/> key to create the <see cref="Uri"/> with.</param>
        /// <returns>A <see cref="Uri"/> with the given <see cref="IUrlToken"/> key embedded.</returns>
        public Uri UrlWithKey(string key)
        {
            if (this.Url == null)
            {
                throw new InvalidOperationException("Url must be set to a value.");
            }

            return new Uri(Url.ToString());
        }
    }
}
