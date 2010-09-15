//-----------------------------------------------------------------------
// <copyright file="UrlToken.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web.UrlTokens
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;
    using Tasty.Configuration;

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

        #region Private Fields

        private DateTime? expires;

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets or sets a value indicating when the token expires, in UTC.
        /// </summary>
        [IgnoreDataMember]
        public DateTime Expires
        {
            get
            {
                return (DateTime)(this.expires ?? (this.expires = DateTime.UtcNow.AddHours(this.ExpiryHours)));
            }

            set
            {
                if (value.Kind != DateTimeKind.Utc)
                {
                    throw new ArgumentException("value must be in UTC.", "value");
                }

                this.expires = value;
            }
        }

        /// <summary>
        /// Gets the number of hours from creation the URL token expires in.
        /// </summary>
        [IgnoreDataMember]
        public virtual int ExpiryHours
        {
            get { return TastySettings.Section.UrlTokens.DefaultExpiryHours; }
        }

        /// <summary>
        /// Gets a value indicating whether the token is expired.
        /// </summary>
        [IgnoreDataMember]
        public bool IsExpired
        {
            get { return this.Expires > DateTime.UtcNow; }
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
            DataContractSerializer serializer = new DataContractSerializer(GetType());
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

        #endregion
    }
}
