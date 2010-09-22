//-----------------------------------------------------------------------
// <copyright file="GitHubWebhook.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.GitHub
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;

    /// <summary>
    /// Represents a GitHub post-receive webhook.
    /// </summary>
    [DataContract]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GitHubWebhook
    {
        private static Type[] knownTypes = new Type[] 
        {
            typeof(GitHubWebhookCommit),
            typeof(GitHubWebhookPerson),
            typeof(GitHubWebhookRepository)
        };

        private GitHubWebhookCommit[] commits;

        /// <summary>
        /// Gets or sets the commit ID of the ref after the last commit.
        /// </summary>
        [DataMember(Name = "after")]
        public string After { get; set; }

        /// <summary>
        /// Gets or sets the commit ID of the ref before the first commit.
        /// </summary>
        [DataMember(Name = "before")]
        public string Before { get; set; }

        /// <summary>
        /// Gets or sets the commits that make up the push.
        /// </summary>
        [DataMember(Name = "commits")]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Meant for serialization.")]
        public GitHubWebhookCommit[] Commits
        {
            get { return this.commits ?? (this.commits = new GitHubWebhookCommit[0]); }
            set { this.commits = value; }
        }

        /// <summary>
        /// Gets or sets the ref that was pushed.
        /// </summary>
        [DataMember(Name = "ref")]
        public string Ref { get; set; }

        /// <summary>
        /// Gets or sets the repository that the push was for.
        /// </summary>
        [DataMember(Name = "repository")]
        public GitHubWebhookRepository Repository { get; set; }

        /// <summary>
        /// De-serializes the given JSON string into a <see cref="GitHubWebhook"/>.
        /// </summary>
        /// <param name="value">The string value to de-serialize.</param>
        /// <returns>The de-serialized <see cref="GitHubWebhook"/>.</returns>
        public static GitHubWebhook Deserialize(string value)
        {
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
            {
                return Deserialize(stream);
            }
        }

        /// <summary>
        /// De-serializes the given stream of JSON into a <see cref="GitHubWebhook"/>.
        /// </summary>
        /// <param name="stream">The stream to de-serialize.</param>
        /// <returns>The de-serialized <see cref="GitHubWebhook"/>.</returns>
        public static GitHubWebhook Deserialize(Stream stream)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GitHubWebhook), knownTypes);
            return (GitHubWebhook)serializer.ReadObject(stream);
        }
    }
}
