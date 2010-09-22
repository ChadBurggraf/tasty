//-----------------------------------------------------------------------
// <copyright file="ClientCertificateElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
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
        /// Gets or sets the name the client certificate is referred to by.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        /// Gets or sets the path of the client certificate file.
        /// </summary>
        [ConfigurationProperty("path", IsRequired = false)]
        public string Path
        {
            get { return (string)this["path"]; }
            set { this["path"] = value; }
        }

        /// <summary>
        /// Gets or sets the password to use when loading the client certificate file.
        /// </summary>
        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get { return (string)this["password"]; }
            set { this["password"] = value; }
        }

        /// <summary>
        /// Gets or sets the fully qualified name of the certificate's embedded resource.
        /// </summary>
        [ConfigurationProperty("resourceName", IsRequired = false)]
        public string ResourceName
        {
            get { return (string)this["resourceName"]; }
            set { this["resourceName"] = value; }
        }

        /// <summary>
        /// Gets or sets the type name identifying the assembly the certificate is embedded into.
        /// </summary>
        [ConfigurationProperty("resourceType", IsRequired = false)]
        public string ResourceType
        {
            get { return (string)this["resourceType"]; }
            set { this["resourceType"] = value; }
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
