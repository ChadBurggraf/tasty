//-----------------------------------------------------------------------
// <copyright file="ServiceElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents a whitelist certificate service configuration element.
    /// </summary>
    public class ServiceElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the service's client certificate element collection.
        /// </summary>
        [ConfigurationProperty("clientCertificates", IsDefaultCollection = true)]
        public ClientCertificateElementCollection ClientCertificates
        {
            get { return (ClientCertificateElementCollection)(this["clientCertificates"] ?? (this["clientCertificates"] = new ClientCertificateElementCollection())); }
        }

        /// <summary>
        /// Gets or sets the service's name.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        /// Gets or sets the path of the server certificate file.
        /// </summary>
        [ConfigurationProperty("serverCertificate", IsRequired = true)]
        public string ServerCertificate
        {
            get { return (string)this["serverCertificate"]; }
            set { this["serverCertificate"] = value; }
        }
    }
}
