//-----------------------------------------------------------------------
// <copyright file="ApplicationElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.JobService
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents an application configuration element.
    /// </summary>
    public class ApplicationElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the path of the configuration file to use when configurint the job runner for the target application.
        /// </summary>
        [ConfigurationProperty("cfgFile", IsRequired = true)]
        public string CfgFile
        {
            get { return (string)this["cfgFile"]; }
            set { this["cfgFile"] = value; }
        }

        /// <summary>
        /// Gets or sets the path of the target application to run jobs for.
        /// </summary>
        [ConfigurationProperty("directory", IsRequired = true)]
        public string Directory
        {
            get { return (string)this["directory"]; }
            set { this["directory"] = value; }
        }

        /// <summary>
        /// Gets or sets the display/reference name of the target application to run jobs for.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }
    }
}
