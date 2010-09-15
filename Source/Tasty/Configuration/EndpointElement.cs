//-----------------------------------------------------------------------
// <copyright file="EndpointElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
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
        [ConfigurationProperty("clientCertificatePath", IsRequired = false)]
        public string ClientCertificatePath
        {
            get { return (string)this["clientCertificatePath"]; }
            set { this["clientCertificatePath"] = value; }
        }

        /// <summary>
        /// Gets or sets the password to use when loading the client certificate file.
        /// </summary>
        [ConfigurationProperty("clientCertificatePassword", IsRequired = false)]
        public string ClientCertificatePassword
        {
            get { return (string)this["clientCertificatePassword"]; }
            set { this["clientCertificatePassword"] = value; }
        }

        /// <summary>
        /// Gets or sets the fully qualified name of the embedded client certificate resource.
        /// </summary>
        [ConfigurationProperty("clientCertificateResourceName", IsRequired = false)]
        public string ClientCertificateResourceName
        {
            get { return (string)this["clientCertificateResourceName"]; }
            set { this["clientCertificateResourceName"] = value; }
        }

        /// <summary>
        /// Gets or sets the type name identifying the assembly the client certificate is embedded into.
        /// </summary>
        [ConfigurationProperty("clientCertificateResourceType", IsRequired = false)]
        public string ClientCertificateResourceType
        {
            get { return (string)this["clientCertificateResourceType"]; }
            set { this["clientCertificateResourceType"] = value; }
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
