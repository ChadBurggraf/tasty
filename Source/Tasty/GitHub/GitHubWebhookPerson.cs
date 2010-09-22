//-----------------------------------------------------------------------
// <copyright file="GitHubWebhookPerson.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.GitHub
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an author in a GitHub webhook.
    /// </summary>
    [DataContract]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GitHubWebhookPerson
    {
        private string email, name;

        /// <summary>
        /// Gets or sets the author's email.
        /// </summary>
        [DataMember(Name = "email")]
        public string Email
        {
            get { return this.email ?? String.Empty; }
            set { this.email = value; }
        }

        /// <summary>
        /// Gets or sets the author's name.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name
        {
            get { return this.name ?? String.Empty; }
            set { this.name = value; }
        }
    }
}
