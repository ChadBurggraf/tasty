using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Tasty.Build
{
    /// <summary>
    /// Extends <see cref="Task"/> to publish static assets to Amazon S3.
    /// Very primitive implementation: does not allow gzip toggling or ACL setting.
    /// </summary>
    public class S3Publish : Task, IS3PublisherDelegate
    {
        #region Member Variables

        private List<ITaskItem> filesPublished;

        #endregion

        #region Properties

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
        /// Gets the collection of files that were published.
        /// </summary>
        [Output]
        public ITaskItem[] FilesPublished { get; set; }

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

        #region Instance Methods

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>True if the task executed successfully, false otherwise.</returns>
        public override bool Execute()
        {
            filesPublished = new List<ITaskItem>();

            new S3Publisher(AccessKeyId, SecretAccessKeyId)
                .WithBasePath(BasePath)
                .WithBucketName(BucketName)
                .WithFiles(Files.Select(ti => ti.ItemSpec))
                .WithPrefix(Prefix)
                .WithPublisherDelegate(this)
                .WithUseSsl(UseSsl).Publish();

            FilesPublished = filesPublished.ToArray();

            return true;
        }

        #endregion

        #region IS3PublisherDelegate Members

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
            filesPublished.Add(item);

            string message = "{0} published to {1}";
            message += withGzip ? " with gzip compression" : String.Empty;
            message += ".";

            Log.LogMessage(message, path, objectKey);
        }

        #endregion
    }
}
