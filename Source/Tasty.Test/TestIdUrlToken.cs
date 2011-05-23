//-----------------------------------------------------------------------
// <copyright file="TestIdUrlToken.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Tasty.Web.UrlTokens;

    /// <summary>
    /// Test implementation of <see cref="UrlToken"/>.
    /// </summary>
    [DataContract(Namespace = UrlToken.XmlNamespace)]
    internal class TestIdUrlToken : UrlToken
    {
        [DataMember]
        private IDictionary<string, string> metadata;

        /// <summary>
        /// Initializes a new instance of the TestIdUrlToken class.
        /// </summary>
        public TestIdUrlToken()
        {
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Gets the number of hours the token is valid for.
        /// </summary>
        public override int ExpiryHours
        {
            get { return 1; }
        }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        public IDictionary<string, string> Metadata
        {
            get { return this.metadata ?? (this.metadata = new Dictionary<string, string>()); }
        }
    }
}
