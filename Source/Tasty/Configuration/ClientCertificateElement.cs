//-----------------------------------------------------------------------
// <copyright file="ClientCertificateElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents a whitelist certificate client configuration element.
    /// </summary>
    public class ClientCertificateElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the path of the server certificate file.
        /// </summary>
        [ConfigurationProperty("clientCertificate", IsRequired = true, IsKey = true)]
        public string ClientCertificate
        {
            get { return (string)this["clientCertificate"]; }
            set { this["clientCertificate"] = value; }
        }
    }
}
