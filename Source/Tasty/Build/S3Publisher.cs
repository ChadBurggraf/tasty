//-----------------------------------------------------------------------
// <copyright file="S3Publisher.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Build
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Net;
    using Amazon;
    using Amazon.S3;
    using Amazon.S3.Model;
    using Ionic.Zlib;
    using Tasty.Web;

    /// <summary>
    /// Publishes static assets to an Amazon S3 bucket.
    /// Compresses files with text-based content types (*.css, *.js, etc.)
    /// and publishes them with a gzip content encoding.
    /// </summary>
    public class S3Publisher : IS3PublisherDelegate
    {
        #region Private Fields

        private string accessKeyId, secretAccessKeyId;
        private bool useSsl;
        private AmazonS3 client;
        private IList<string> files;
        private IS3PublisherDelegate publisherDelegate;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the S3Publisher class.
        /// </summary>
        /// <param name="accessKeyId">The Amazon S3 access key ID to use when connecting to the service.</param>
        /// <param name="secretAccessKeyId">The Amazon S3 secret access key ID to use when connecting to the service.</param>
        public S3Publisher(string accessKeyId, string secretAccessKeyId)
        {
            if (String.IsNullOrEmpty(accessKeyId))
            {
                throw new ArgumentNullException("accessKeyId", "accessKeyId must have a value.");
            }

            if (String.IsNullOrEmpty(secretAccessKeyId))
            {
                throw new ArgumentNullException("secretAccessKeyId", "secretAccessKeyId must have a value.");
            }

            this.accessKeyId = accessKeyId;
            this.secretAccessKeyId = secretAccessKeyId;
            this.OverwriteExisting = true;
            this.OverwriteExistingPrefix = true;
        }

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets or sets the base path to use when relativising file paths to publish.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Gets or sets the bucket name to use when publishing files.
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// Gets the collection of files to publish.
        /// </summary>
        public IList<string> Files
        {
            get { return this.files ?? (this.files = new List<string>()); }
        }

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
        /// Gets or sets the prefix to use as the root path for the published directory on S3.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the delegate to use when posting publish notifications.
        /// </summary>
        public IS3PublisherDelegate PublisherDelegate
        {
            get { return this.publisherDelegate ?? (this.publisherDelegate = this); }
            set { this.publisherDelegate = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use SSL when connecting to the service.
        /// </summary>
        public bool UseSsl
        {
            get 
            { 
                return this.useSsl; 
            }
            set
            {
                this.useSsl = value;
                this.client = null;
            }
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Called when a file has been successfully published to Amazon S3.
        /// </summary>
        /// <param name="path">The path of the file that was published.</param>
        /// <param name="objectKey">The resulting object key of the file on Amazon S3.</param>
        /// <param name="withGzip">A value indicating whether the file was compressed with GZip before publishing.</param>
        public void OnFilePublished(string path, string objectKey, bool withGzip)
        {
        }

        /// <summary>
        /// Called when a file is skipped for publishing because it already exists.
        /// </summary>
        /// <param name="path">The path of the file that was skipped.</param>
        /// <param name="objectKey">The object key of the file on the service.</param>
        public void OnFileSkipped(string path, string objectKey)
        {
        }

        /// <summary>
        /// Called when an entire prefix is skipped for publishing because it already exists.
        /// </summary>
        /// <param name="prefix">The prefix that was skipped.</param>
        public void OnPrefixSkipped(string prefix)
        {
        }

        /// <summary>
        /// Publishes the currently-identified file set to Amazon S3.
        /// </summary>
        public void Publish()
        {
            if (String.IsNullOrEmpty(this.BucketName))
            {
                throw new InvalidOperationException("BucketName must have a value.");
            }

            if (this.OverwriteExistingPrefix || !this.PrefixExists())
            {
                foreach (string filePath in this.Files)
                {
                    this.PublishFile(filePath);
                }
            }
            else
            {
                this.PublisherDelegate.OnPrefixSkipped(this.Prefix);
            }
        }

        /// <summary>
        /// Sets the value of <see cref="BasePath"/> and returns this instance.
        /// </summary>
        /// <param name="basePath">The value to set.</param>
        /// <returns>This instance.</returns>
        public S3Publisher WithBasePath(string basePath)
        {
            this.BasePath = basePath;
            return this;
        }

        /// <summary>
        /// Sets the value of <see cref="BucketName"/> and returns this instance.
        /// </summary>
        /// <param name="bucketName">The value to set.</param>
        /// <returns>This instance.</returns>
        public S3Publisher WithBucketName(string bucketName)
        {
            this.BucketName = bucketName;
            return this;
        }

        /// <summary>
        /// Clears the <see cref="Files"/> collection and then fills it with the given collection and returns this instance.
        /// </summary>
        /// <param name="files">The new file collection.</param>
        /// <returns>This instance.</returns>
        public S3Publisher WithFiles(IEnumerable<string> files)
        {
            List<string> list = new List<string>();

            if (files != null)
            {
                list.AddRange(files.ToArray());
            }

            this.files = list;
            return this;
        }

        /// <summary>
        /// Sets the value of <see cref="OverwriteExisting"/> and returns this instance.
        /// </summary>
        /// <param name="overwriteExisting">The value to set.</param>
        /// <returns>This instance.</returns>
        public S3Publisher WithOverwriteExisting(bool overwriteExisting)
        {
            this.OverwriteExisting = overwriteExisting;
            return this;
        }

        /// <summary>
        /// Sets the value of <see cref="OverwriteExistingPrefix"/> and returns this instance.
        /// </summary>
        /// <param name="overwriteExistingPrefix">The value to set.</param>
        /// <returns>This instance.</returns>
        public S3Publisher WithOverwriteExistingPrefix(bool overwriteExistingPrefix)
        {
            this.OverwriteExistingPrefix = overwriteExistingPrefix;
            return this;
        }

        /// <summary>
        /// Sets the value of <see cref="Prefix"/> and returns this instance.
        /// </summary>
        /// <param name="prefix">The value to set.</param>
        /// <returns>This instance.</returns>
        public S3Publisher WithPrefix(string prefix)
        {
            this.Prefix = prefix;
            return this;
        }

        /// <summary>
        /// Sets the value of <see cref="PublisherDelegate"/> and returns this instance.
        /// </summary>
        /// <param name="publisherDelegate">The value to set.</param>
        /// <returns>This instance.</returns>
        public S3Publisher WithPublisherDelegate(IS3PublisherDelegate publisherDelegate)
        {
            this.PublisherDelegate = publisherDelegate;
            return this;
        }

        /// <summary>
        /// Sets the value of <see cref="UseSsl"/> and returns this instance.
        /// </summary>
        /// <param name="useSsl">The value to set.</param>
        /// <returns>This instance.</returns>
        public S3Publisher WithUseSsl(bool useSsl)
        {
            this.UseSsl = useSsl;
            return this;
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Gets a <see cref="AmazonS3"/> client base on this instance's current state.
        /// </summary>
        /// <returns>An <see cref="AmazonS3"/> client.</returns>
        private AmazonS3 Client()
        {
            if (this.client == null)
            {
                AmazonS3Config config = new AmazonS3Config();
                config.CommunicationProtocol = this.UseSsl ? Protocol.HTTPS : Protocol.HTTP;

                this.client = AWSClientFactory.CreateAmazonS3Client(this.accessKeyId, this.secretAccessKeyId, config);
            }

            return this.client;
        }

        /// <summary>
        /// Gets a value indicating whether the object with the given key exists on the service.
        /// </summary>
        /// <param name="key">The key to check the existence of.</param>
        /// <returns>True if the object exists, false otherwise.</returns>
        private bool ObjectExists(string key)
        {
            bool exists = false;

            GetObjectMetadataRequest request = new GetObjectMetadataRequest()
                .WithBucketName(this.BucketName)
                .WithKey(key);

            try
            {
                using (GetObjectMetadataResponse response = this.Client().GetObjectMetadata(request))
                {
                    exists = true;
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound || !"NoSuchKey".Equals(ex.ErrorCode, StringComparison.OrdinalIgnoreCase))
                {
                    throw;
                }
            }

            return exists;
        }

        /// <summary>
        /// Gets the object key to use for the file at the given path.
        /// </summary>
        /// <param name="filePath">The file path to get the object key for.</param>
        /// <returns>The file's object key.</returns>
        private string ObjectKey(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath", "filePath must have a value.");
            }

            string basePath = (this.BasePath ?? String.Empty).Trim();
            string prefix = (this.Prefix ?? String.Empty).Trim();
            string key;

            if (!Path.IsPathRooted(filePath))
            {
                filePath = Path.GetFullPath(filePath);
            }

            if (!String.IsNullOrEmpty(basePath) && !Path.IsPathRooted(basePath))
            {
                basePath = Path.GetFullPath(basePath);
            }

            if (!String.IsNullOrEmpty(basePath) && filePath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                key = filePath.Substring(basePath.Length).Replace(@"\", "/");
            }
            else
            {
                key = Path.GetFileName(filePath);
            }

            if (!String.IsNullOrEmpty(prefix) && !key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                key = Uris.Combine(prefix, key);
            }

            return key;
        }

        /// <summary>
        /// Gets a value indicating whether this instance's prefix exists.
        /// </summary>
        /// <returns>True if the prefix exists, false otherwise.</returns>
        private bool PrefixExists()
        {
            bool exists = false;
            string prefix = (this.Prefix ?? String.Empty).Trim();

            if (!String.IsNullOrEmpty(prefix))
            {
                ListObjectsRequest request = new ListObjectsRequest()
                    .WithBucketName(this.BucketName)
                    .WithPrefix(prefix)
                    .WithMaxKeys(1);

                using (ListObjectsResponse response = this.Client().ListObjects(request))
                {
                    return response.S3Objects != null && response.S3Objects.Count > 0;
                }
            }

            return exists;
        }

        /// <summary>
        /// Publishes a file to Amazon S3.
        /// </summary>
        /// <param name="filePath">The path of the file to publish.</param>
        private void PublishFile(string filePath)
        {
            NameValueCollection headers = new NameValueCollection();
            string contentType = MimeType.FromCommon(filePath).ContentType;
            string objectKey = this.ObjectKey(filePath);

            if (this.OverwriteExisting || !this.ObjectExists(objectKey))
            {
                PutObjectRequest request = new PutObjectRequest()
                    .WithBucketName(this.BucketName)
                    .WithCannedACL(S3CannedACL.PublicRead)
                    .WithContentType(contentType)
                    .WithKey(objectKey);

                bool gzip = false;
                string tempPath = null;

                if (contentType.StartsWith("text", StringComparison.OrdinalIgnoreCase))
                {
                    gzip = true;
                    tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Path.GetRandomFileName());

                    using (FileStream fs = File.OpenRead(filePath))
                    {
                        using (FileStream temp = File.Create(tempPath))
                        {
                            using (GZipStream gz = new GZipStream(temp, CompressionMode.Compress))
                            {
                                byte[] buffer = new byte[4096];
                                int count;

                                while (0 < (count = fs.Read(buffer, 0, buffer.Length)))
                                {
                                    gz.Write(buffer, 0, count);
                                }
                            }
                        }
                    }

                    headers["Content-Encoding"] = "gzip";
                    request = request.WithFilePath(tempPath);
                }
                else
                {
                    request = request.WithFilePath(filePath);
                }

                request.AddHeaders(headers);

                using (PutObjectResponse response = this.Client().PutObject(request))
                {
                }

                if (!String.IsNullOrEmpty(tempPath) && File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                this.PublisherDelegate.OnFilePublished(filePath, objectKey, gzip);
            }
            else
            {
                this.PublisherDelegate.OnFileSkipped(filePath, objectKey);
            }
        }

        #endregion
    }
}
