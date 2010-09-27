﻿//-----------------------------------------------------------------------
// <copyright file="ApplicationElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.JobService
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents an application configuration element.
    /// </summary>
    public class ApplicationElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the path of the configuration file to use when configurint the job runner for the target application.
        /// </summary>
        [ConfigurationProperty("cfgFile", IsRequired = true)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Config/configuration is a reserved word in configuration files.")]
        public string CfgFile
        {
            get { return (string)this["cfgFile"]; }
            set { this["cfgFile"] = value; }
        }

        /// <summary>
        /// Gets or sets the path of the target application to run jobs for.
        /// </summary>
        [ConfigurationProperty("binDirectory", IsRequired = true)]
        public string BinDirectory
        {
            get { return (string)this["binDirectory"]; }
            set { this["binDirectory"] = value; }
        }

        /// <summary>
        /// Gets or sets the path override to use for the log file, if overriding is desired.
        /// </summary>
        [ConfigurationProperty("logFile", IsRequired = false)]
        public string LogFile
        {
            get { return (string)this["logFile"]; }
            set { this["logFile"] = value; }
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
