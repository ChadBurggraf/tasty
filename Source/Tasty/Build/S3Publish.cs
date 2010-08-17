//-----------------------------------------------------------------------
// <copyright file="S3Publish.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Build
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// Extends <see cref="Task"/> to publish static assets to Amazon S3.
    /// Very primitive implementation: does not allow gzip toggling or ACL setting.
    /// </summary>
    public class S3Publish : Task, IS3PublisherDelegate
    {
        #region Private Fields

        private List<ITaskItem> filesPublished;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the S3Publish class.
        /// </summary>
        public S3Publish()
            : base()
        {
            this.OverwriteExisting = true;
            this.OverwriteExistingPrefix = true;
        }

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets or sets the Amazon S3 access key ID to use when connecting to the service.
        /// </summary>
        [Required]
        public string AccessKeyId { get; set; }

        /// <summary>
        /// Gets or sets the base path to use when relativizing published file paths.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Gets or sets the name of the bucket to publish assets to.
        /// </summary>
        [Required]
        public string BucketName { get; set; }

        /// <summary>
        /// Gets or sets the file set to publish.
        /// </summary>
        [Required]
        public ITaskItem[] Files { get; set; }

        /// <summary>
        /// Gets or sets the collection of files that were published.
        /// </summary>
        [Output]
        public ITaskItem[] FilesPublished { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite existing objects.
        /// Defaults to true.
        /// </summary>
        public bool OverwriteExisting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite objects
        /// in the given prefix. If false and <see cref="Prefix"/> is set
        /// and the prefix exists on the service, nothing will be written at all.
        /// Defaults to true.
        /// </summary>
        public bool OverwriteExistingPrefix { get; set; }

        /// <summary>
        /// Gets or sets the object prefix to use for object keys when publishing files.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the Amazon S3 secret access key ID to use when connecting to the service.
        /// </summary>
        [Required]
        public string SecretAccessKeyId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to connect to the service using SSL.
        /// </summary>
        public bool UseSsl { get; set; }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>True if the task executed successfully, false otherwise.</returns>
        public override bool Execute()
        {
            try
            {
                this.filesPublished = new List<ITaskItem>();

                if (this.Files != null && this.Files.Length > 0)
                {
                    new S3Publisher(this.AccessKeyId, this.SecretAccessKeyId)
                        .WithBasePath(this.BasePath)
                        .WithBucketName(this.BucketName)
                        .WithFiles(this.Files.Select(ti => ti.ItemSpec))
                        .WithOverwriteExisting(this.OverwriteExisting)
                        .WithOverwriteExistingPrefix(this.OverwriteExistingPrefix)
                        .WithPrefix(this.Prefix)
                        .WithPublisherDelegate(this)
                        .WithUseSsl(this.UseSsl)
                        .Publish();
                }

                this.FilesPublished = this.filesPublished.ToArray();

                return true;
            }
            catch (Exception ex)
            {
                Log.LogMessage(MessageImportance.High, "Whoops:{0}\r\nStack trace:\r\n{1}", ex.Message, ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Called when a file has been successfully published to Amazon S3.
        /// </summary>
        /// <param name="path">The path of the file that was published.</param>
        /// <param name="objectKey">The resulting object key of the file on Amazon S3.</param>
        /// <param name="withGzip">A value indicating whether the file was compressed with GZip before publishing.</param>
        public void OnFilePublished(string path, string objectKey, bool withGzip)
        {
            ITaskItem item = new TaskItem(objectKey);
            item.SetMetadata("Path", path);
            item.SetMetadata("WithGzip", withGzip.ToString(CultureInfo.InvariantCulture));
            this.filesPublished.Add(item);

            string message = "{0} published to {1}";
            message += withGzip ? " with gzip compression" : String.Empty;
            message += ".";

            Log.LogMessage(message, path, objectKey);
        }

        /// <summary>
        /// Called when a file is skipped for publishing because it already exists.
        /// </summary>
        /// <param name="path">The path of the file that was skipped.</param>
        /// <param name="objectKey">The object key of the file on the service.</param>
        public void OnFileSkipped(string path, string objectKey)
        {
            Log.LogMessage("{0} skipped because it exists at {1}.", path, objectKey);
        }

        /// <summary>
        /// Called when an entire prefix is skipped for publishing because it already exists.
        /// </summary>
        /// <param name="prefix">The prefix that was skipped.</param>
        public void OnPrefixSkipped(string prefix)
        {
            Log.LogMessage("Prefix \"{0}\" skipped because it already exists.", prefix);
        }

        #endregion
    }
}
