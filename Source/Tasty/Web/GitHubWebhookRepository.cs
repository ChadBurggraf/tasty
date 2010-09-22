

namespace Tasty.Web
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a repository in a GitHub webhook.
    /// </summary>
    [DataContract]
    public class GitHubWebhookRepository
    {
        private string description, homepage, name, pledgie, url;
        private GitHubWebhookPerson owner;

        /// <summary>
        /// Gets or sets the repository's description.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description
        {
            get { return this.description ?? String.Empty; }
            set { this.description = value; }
        }

        /// <summary>
        /// Gets or sets the number of forks the repository has.
        /// </summary>
        [DataMember(Name = "forks")]
        public int Forks { get; set; }

        /// <summary>
        /// Gets or sets the repository's homepage.
        /// </summary>
        [DataMember(Name = "homepage")]
        public string Homepage
        {
            get { return this.homepage ?? String.Empty; }
            set { this.homepage = value; }
        }

        /// <summary>
        /// Gets or sets the name of the repository.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name
        {
            get { return this.name ?? String.Empty; }
            set { this.name = value; }
        }

        /// <summary>
        /// Gets or sets the repository's owner.
        /// </summary>
        [DataMember(Name = "owner")]
        public GitHubWebhookPerson Owner
        {
            get { return this.owner ?? (this.owner = new GitHubWebhookPerson()); }
            set { this.owner = value; }
        }

        /// <summary>
        /// Gets or sets the repository's pledge ID.
        /// </summary>
        [DataMember(Name = "pledgie")]
        public string Plegie
        {
            get { return this.pledgie ?? String.Empty; }
            set { this.pledgie = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the repository is private.
        /// </summary>
        [DataMember(Name = "private")]
        public bool Private { get; set; }

        /// <summary>
        /// Gets or sets the repository's URL.
        /// </summary>
        [DataMember(Name = "url")]
        public string Url
        {
            get { return this.url ?? String.Empty; }
            set { this.url = value; }
        }

        /// <summary>
        /// Gets or sets the number of watchers the repository has.
        /// </summary>
        [DataMember(Name = "watchers")]
        public int Watchers { get; set; }
    }
}
