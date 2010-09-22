

namespace Tasty.Web
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an author in a GitHub webhook.
    /// </summary>
    [DataContract]
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
