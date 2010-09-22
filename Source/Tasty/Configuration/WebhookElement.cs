//-----------------------------------------------------------------------
// <copyright file="WebhookElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a GitHub webhook element in the configuration.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class WebhookElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the path of the MSBuild project file to execute when a webhook is received.
        /// </summary>
        [ConfigurationProperty("projectFile", IsRequired = true)]
        public string ProjectFile
        {
            get { return (string)this["projectFile"]; }
            set { this["projectFile"] = value; }
        }

        /// <summary>
        /// Gets or sets a regular expression that can be used to filter on the <see cref="Tasty.GitHub.GitHubWebhook.Ref"/> value.
        /// </summary>
        [ConfigurationProperty("refFilter", IsRequired = false)]
        public string RefFilter
        {
            get { return (string)this["refFilter"]; }
            set { this["refFilter"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the repository the element is targeting.
        /// </summary>
        [ConfigurationProperty("repository", IsRequired = true, IsKey = true)]
        public string Repository
        {
            get { return (string)this["repository"]; }
            set { this["repository"] = value; }
        }

        /// <summary>
        /// Gets or sets a semi-colon-delimited list of targets to call in the project file.
        /// Leave empty to call the default target.
        /// </summary>
        [ConfigurationProperty("targets", IsRequired = false)]
        public string Targets
        {
            get { return (string)this["targets"]; }
            set { this["targets"] = value; }
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
