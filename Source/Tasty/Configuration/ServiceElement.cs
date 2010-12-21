//-----------------------------------------------------------------------
// <copyright file="ServiceElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
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
        /// Gets or sets a value indicating whether validation is enabled for the service.
        /// </summary>
        [ConfigurationProperty("enabled", IsRequired = true, DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
            set { this["enabled"] = value; }
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
        [ConfigurationProperty("serverCertificatePath", IsRequired = false)]
        public string ServerCertificatePath
        {
            get { return (string)this["serverCertificatePath"]; }
            set { this["serverCertificatePath"] = value; }
        }

        /// <summary>
        /// Gets or sets the password to use when loading the server certificate file.
        /// </summary>
        [ConfigurationProperty("serverCertificatePassword", IsRequired = false)]
        public string ServerCertificatePassword
        {
            get { return (string)this["serverCertificatePassword"]; }
            set { this["serverCertificatePassword"] = value; }
        }

        /// <summary>
        /// Gets or sets the fully qualified name of the server certificate's embedded resource.
        /// </summary>
        [ConfigurationProperty("serverCertificateResourceName", IsRequired = false)]
        public string ServerCertificateResourceName
        {
            get { return (string)this["serverCertificateResourceName"]; }
            set { this["serverCertificateResourceName"] = value; }
        }

        /// <summary>
        /// Gets or sets the type name identifying the assembly the server certificate is embedded into.
        /// </summary>
        [ConfigurationProperty("serverCertificateResourceType", IsRequired = false)]
        public string ServerCertificateResourceType
        {
            get { return (string)this["serverCertificateResourceType"]; }
            set { this["serverCertificateResourceType"] = value; }
        }

        /// <summary>
        /// Gets a value indicating if the System.Configuration.ConfigurationElement object is read-only.
        /// </summary>
        /// <returns>True if the System.Configuration.ConfigurationElement object is read-only, false otherwise.</returns>
        public override bool IsReadOnly()
        {
            return false;
        }
    }
}
