//-----------------------------------------------------------------------
// <copyright file="UrlToken.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Base <see cref="IUrlToken"/> implementation.
    /// </summary>
    [DataContract(Namespace = UrlToken.XmlNamespace)]
    public abstract class UrlToken : IUrlToken
    {
        #region Public Fields

        /// <summary>
        /// Gets the XML namespace used during token serialization.
        /// </summary>
        public const string XmlNamespace = "http://tastycodes.com/tasty-dll/url-token/";

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets the number of hours from creation the URL token expires in.
        /// </summary>
        [IgnoreDataMember]
        public abstract int ExpiryHours { get; }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Loads and creates the <see cref="IUrlToken"/> from the given <see cref="Uri"/> using the given
        /// <see cref="IUrlTokenUrlProvider"/> and <see cref="IUrlTokenStore"/>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to load the token for.</param>
        /// <param name="urlProvider">The <see cref="IUrlTokenUrlProvider"/> to use when extracting the token key from the <see cref="Uri"/>.</param>
        /// <returns>An <see cref="IUrlToken"/> object, or null if none was found.</returns>
        public static IUrlToken FromUrl(Uri uri, IUrlTokenUrlProvider urlProvider)
        {
            return FromUrl(uri, urlProvider, UrlTokenStore.Current);
        }

        /// <summary>
        /// Loads and creates the <see cref="IUrlToken"/> from the given <see cref="Uri"/> using the given
        /// <see cref="IUrlTokenUrlProvider"/> and <see cref="IUrlTokenStore"/>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to load the token for.</param>
        /// <param name="urlProvider">The <see cref="IUrlTokenUrlProvider"/> to use when extracting the token key from the <see cref="Uri"/>.</param>
        /// <param name="tokenStore">The <see cref="IUrlTokenStore"/> to load data from.</param>
        /// <returns>An <see cref="IUrlToken"/> object, or null if none was found.</returns>
        public static IUrlToken FromUrl(Uri uri, IUrlTokenUrlProvider urlProvider, IUrlTokenStore tokenStore)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri", "uri must have a value.");
            }

            if (urlProvider == null)
            {
                throw new ArgumentNullException("urlProvider", "urlProvider must have a value.");
            }

            if (tokenStore == null)
            {
                throw new ArgumentNullException("tokenStore", "tokenStore must have a value.");
            }

            IUrlToken token = null;
            string key = urlProvider.SeparateKey(uri);

            if (!String.IsNullOrEmpty(key))
            {
                UrlTokenRecord record = tokenStore.GetUrlToken(key);

                if (record != null)
                {
                    token = record.ToUrlToken();
                }
            }

            return token;
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Generates a new unique key that can be used to identify the URL token.
        /// </summary>
        /// <returns>A unique token identifier.</returns>
        public virtual string GenerateKey()
        {
            return Path.GetRandomFileName().Replace(".", String.Empty);
        }

        /// <summary>
        /// Serializes the URL token for storage.
        /// </summary>
        /// <returns>The serialized URL token data.</returns>
        public virtual string Serialize()
        {
            DataContractSerializer serializer = new DataContractSerializer(this.GetType());
            StringBuilder sb = new StringBuilder();

            using (StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                using (XmlWriter xw = new XmlTextWriter(sw))
                {
                    serializer.WriteObject(xw, this);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates a URL that identifies this instance by persisting
        /// this instance to the currently configured <see cref="IUrlTokenStore"/> and
        /// using the given <see cref="IUrlTokenUrlProvider"/> to generate a <see cref="Uri"/>.
        /// </summary>
        /// <param name="urlProvider">The <see cref="IUrlTokenUrlProvider"/> to use when generating the <see cref="Uri"/>.</param>
        /// <returns>A <see cref="Uri"/> that identifies this instance.</returns>
        public Uri ToUrl(IUrlTokenUrlProvider urlProvider)
        {
            return this.ToUrl(urlProvider, UrlTokenStore.Current);
        }

        /// <summary>
        /// Creates a URL that identifies this instance by persisting
        /// this instance to the given <see cref="IUrlTokenStore"/> and
        /// using the given <see cref="IUrlTokenUrlProvider"/> to generate
        /// a <see cref="Uri"/>.
        /// </summary>
        /// <param name="urlProvider">The <see cref="IUrlTokenUrlProvider"/> to use when generating the <see cref="Uri"/>.</param>
        /// <param name="tokenStore">The <see cref="IUrlTokenStore"/> to use when persisting the token's data.</param>
        /// <returns>A <see cref="Uri"/> that identifies this instance.</returns>
        public virtual Uri ToUrl(IUrlTokenUrlProvider urlProvider, IUrlTokenStore tokenStore)
        {
            if (urlProvider == null)
            {
                throw new ArgumentNullException("urlProvider", "urlProvider must have a value.");
            }

            if (tokenStore == null)
            {
                throw new ArgumentNullException("tokenStore", "tokenStore must have a value.");
            }

            string key = this.GenerateKey();

            tokenStore.CreateUrlToken(new UrlTokenRecord()
            {
                Created = DateTime.UtcNow,
                Data = this.Serialize(),
                Expires = DateTime.UtcNow.AddHours(this.ExpiryHours),
                Key = key,
                TokenType = this.GetType()
            });

            return urlProvider.UrlWithKey(key);
        }

        #endregion
    }
}
