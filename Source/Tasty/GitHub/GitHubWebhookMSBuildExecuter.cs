//-----------------------------------------------------------------------
// <copyright file="GitHubWebhookMSBuildExecuter.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.GitHub
{
    using System;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Build.BuildEngine;
    using Tasty.Web;

    /// <summary>
    /// Executes an MSBuild project with properties and items injected from a <see cref="GitHubWebhook"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GitHubWebhookMSBuildExecuter
    {
        private string logFile;
        private StringCollection targets;

        /// <summary>
        /// Initializes a new instance of the GitHubWebhookMSBuildExecuter class.
        /// </summary>
        /// <param name="projectFile">The path to the MSBuild project file to execute.</param>
        /// <param name="hook">The <see cref="GitHubWebhook"/> to inject properties and items from.</param>
        public GitHubWebhookMSBuildExecuter(string projectFile, GitHubWebhook hook)
        {
            if (String.IsNullOrEmpty(projectFile))
            {
                throw new ArgumentNullException("projectFile", "projectFile must contain a value.");
            }

            projectFile = projectFile.RootPath();

            if (!File.Exists(projectFile))
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, @"The path ""{0}"" does not exist.", projectFile), "projectFile");
            }

            if (hook == null)
            {
                throw new ArgumentNullException("hook", "hook cannot be null.");
            }

            this.Hook = hook;
            this.ProjectFile = projectFile;
        }

        /// <summary>
        /// Gets the <see cref="GitHubWebhook"/> properties and items are being injected from.
        /// </summary>
        public GitHubWebhook Hook { get; private set; }

        /// <summary>
        /// Gets or sets the path to log MSBuild output to, if desired.
        /// </summary>
        public string LogFile
        {
            get 
            { 
                return this.logFile ?? String.Empty; 
            }

            set
            {
                this.logFile = (value ?? String.Empty).RootPath();
            }
        }

        /// <summary>
        /// Gets the path of the MSBuild project file being executed.
        /// </summary>
        public string ProjectFile { get; private set; }

        /// <summary>
        /// Gets the collection of targets to call when executing the project.
        /// </summary>
        public StringCollection Targets
        {
            get { return this.targets ?? (this.targets = new StringCollection()); }
        }

        /// <summary>
        /// Prepares the given MSBuild <see cref="Project"/> with properties and items from the given <see cref="GitHubWebhook"/>.
        /// </summary>
        /// <param name="project">The project to prepare.</param>
        /// <param name="hook">The webhook to prepare the project with.</param>
        public static void PrepareProject(Project project, GitHubWebhook hook)
        {
            BuildPropertyGroup properties = project.AddNewPropertyGroup(false);
            properties.AddNewProperty("GitHubAfter", hook.After);
            properties.AddNewProperty("GitHubBefore", hook.Before);
            properties.AddNewProperty("GitHubRef", hook.Ref);
            properties.AddNewProperty("GitHubRepositoryDescription", hook.Repository.Description);
            properties.AddNewProperty("GitHubRepositoryForks", hook.Repository.Forks.ToString(CultureInfo.InvariantCulture));
            properties.AddNewProperty("GitHubRepositoryHomepage", hook.Repository.Homepage);
            properties.AddNewProperty("GitHubRepositoryName", hook.Repository.Name);
            properties.AddNewProperty("GitHubRepositoryOwnerName", hook.Repository.Owner.Name);
            properties.AddNewProperty("GitHubRepositoryOwnerEmail", hook.Repository.Owner.Email);
            properties.AddNewProperty("GitHubRepositoryPlegie", hook.Repository.Plegie);
            properties.AddNewProperty("GitHubRepositoryPrivate", hook.Repository.Private.ToString(CultureInfo.InvariantCulture));
            properties.AddNewProperty("GitHubRepositoryUrl", hook.Repository.Url);
            properties.AddNewProperty("GitHubRepositoryWatchers", hook.Repository.Watchers.ToString(CultureInfo.InvariantCulture));

            BuildItemGroup commits = project.AddNewItemGroup();

            foreach (GitHubWebhookCommit commit in hook.Commits)
            {
                BuildItem item = commits.AddNewItem("GitHubCommit", commit.Url);
                item.SetMetadata("Id", commit.Id);
                item.SetMetadata("Message", commit.Message);
                item.SetMetadata("Timestamp", commit.Timestamp);
                item.SetMetadata("AuthorName", commit.Author.Name);
                item.SetMetadata("AuthorEmail", commit.Author.Email);
            }
        }

        /// <summary>
        /// Executes this instance's MSBuild project file.
        /// </summary>
        /// <returns>True if the project executed successfully, false otherwise.</returns>
        public bool Execute()
        {
            Engine engine = new Engine();
            bool success = false;
            string currentDir = Environment.CurrentDirectory;

            try
            {
                Environment.CurrentDirectory = Path.GetDirectoryName(this.ProjectFile);

                if (!String.IsNullOrEmpty(this.LogFile))
                {
                    FileLogger logger = new FileLogger();
                    logger.Parameters = String.Concat("logfile=", this.LogFile);
                    engine.RegisterLogger(logger);
                }

                Project project = new Project(engine);
                project.Load(this.ProjectFile);

                PrepareProject(project, this.Hook);

                string[] targets = null;

                if (this.Targets.Count > 0)
                {
                    targets = this.Targets.Cast<string>().ToArray();
                }

                success = project.Build(targets);
            }
            finally
            {
                engine.UnregisterAllLoggers();
                Environment.CurrentDirectory = currentDir;
            }

            return success;
        }

        /// <summary>
        /// Sets the <see cref="Targets"/> collection from the given semi-colon-delimited string.
        /// </summary>
        /// <param name="targets">A string of targets.</param>
        public void SetTargets(string targets)
        {
            this.Targets.Clear();

            if (!String.IsNullOrEmpty(targets))
            {
                this.Targets.AddRange(targets.SplitAndTrim(';').Where(t => !String.IsNullOrEmpty(t)).ToArray());
            }
        }
    }
}
