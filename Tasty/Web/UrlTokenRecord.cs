//-----------------------------------------------------------------------
// <copyright file="UrlTokenRecord.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml;

    /// <summary>
    /// Represents a URL token record in persistent storage.
    /// </summary>
    [Serializable]
    public sealed class UrlTokenRecord
    {
        /// <summary>
        /// Initializes a new instance of the UrlTokenRecord class.
        /// </summary>
        public UrlTokenRecord()
        {
        }

        /// <summary>
        /// Initializes a new instance of the UrlTokenRecord class.
        /// </summary>
        /// <param name="record">The prototype <see cref="UrlTokenRecord"/> to initialize this instance from.</param>
        public UrlTokenRecord(UrlTokenRecord record)
        {
            if (record != null)
            {
                this.Created = record.Created;
                this.Data = record.Data;
                this.Expires = record.Expires;
                this.Key = record.Key;
                this.TokenType = record.TokenType;
            }
        }

        /// <summary>
        /// Gets or sets the date the token was created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the serialized token data (i.e., from calling <see cref="IUrlToken.Serialize"/>.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Gets or sets the date the token expires.
        /// </summary>
        public DateTime Expires { get; set; }

        /// <summary>
        /// Gets or sets the token's unique key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets <see cref="IUrlToken"/> implementor that the token is persisted for.
        /// </summary>
        public Type TokenType { get; set; }

        /// <summary>
        /// Converts this instance's <see cref="TokenType"/> and <see cref="Data"/> properties into an <see cref="IUrlToken"/> object.
        /// </summary>
        /// <returns>An <see cref="IUrlToken"/> object.</returns>
        public IUrlToken ToUrlToken()
        {
            if (this.TokenType == null)
            {
                throw new InvalidOperationException("TokenType must have a value in order to convert this instance's Data property into an IUrlToken object.");
            }

            if (String.IsNullOrEmpty(this.Data))
            {
                throw new InvalidOperationException("Data must have a value to de-serialize an IUrlToken object from.");
            }

            DataContractSerializer serializer = new DataContractSerializer(this.TokenType);

            using (StringReader sr = new StringReader(this.Data))
            {
                using (XmlReader xr = new XmlTextReader(sr))
                {
                    return (IUrlToken)serializer.ReadObject(xr);
                }
            }
        }
    }
}
