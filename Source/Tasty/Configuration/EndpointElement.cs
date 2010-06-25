//-----------------------------------------------------------------------
// <copyright file="EndpointElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents a whitelist certificate client service endpoint configuration element.
    /// </summary>
    public class EndpointElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the path of the client certificate file.
        /// </summary>
        [ConfigurationProperty("clientCertificate", IsRequired = true)]
        public string ClientCertificate
        {
            get { return (string)this["clientCertificate"]; }
            set { this["clientCertificate"] = value; }
        }

        /// <summary>
        /// Gets or sets the service contract the client certificate is for.
        /// </summary>
        [ConfigurationProperty("contract", IsRequired = false, IsKey = true)]
        public string Contract
        {
            get { return (string)this["contract"]; }
            set { this["contract"] = value; }
        }
    }
}
