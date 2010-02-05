//-----------------------------------------------------------------------
// <copyright file="UrlTokenRecord.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web
{
    using System;

    /// <summary>
    /// Represents a URL token record in persistent storage.
    /// </summary>
    [Serializable]
    public sealed class UrlTokenRecord
    {
        /// <summary>
        /// Initializes a new instance of the JobRecord class.
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
    }
}
