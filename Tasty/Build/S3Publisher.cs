using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Ionic.Zlib;
using Tasty.Web;

namespace Tasty.Build
{
    /// <summary>
    /// Publishes static assets to an Amazon S3 bucket.
    /// Compresses files with text-based content types (*.css, *.js, etc.)
    /// and publishes them with a "gzip" content encoding.
    /// </summary>
    public class S3Publisher : IS3PublisherDelegate
    {
        #region Member Variables

        private string accessKeyId, secretAccessKeyId;
        private IList<string> files;
        private IS3PublisherDelegate publisherDelegate;

        #endregion

        #region Construction

        /// <summary>
        /// Constructor.
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
        }

        #endregion

        #region Properties

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
            get { return files ?? (files = new List<string>()); }
        }

        /// <summary>
        /// Gets or sets the prefix to use as the root path for the published directory on S3.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the delegate to use when posting publish notifications.
        /// </summary>
        public IS3PublisherDelegate PublisherDelegate
        {
            get { return publisherDelegate ?? (publisherDelegate = this); }
            set { publisherDelegate = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use SSL when connecting to the service.
        /// </summary>
        public bool UseSsl { get; set; }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Gets a new <see cref="AmazonS3"/> client base on this instance's current state.
        /// </summary>
        /// <returns>An <see cref="AmazonS3"/> client.</returns>
        private AmazonS3 Client()
        {
            AmazonS3Config config = new AmazonS3Config();
            config.CommunicationProtocol = UseSsl ? Protocol.HTTPS : Protocol.HTTP;
            
            return AWSClientFactory.CreateAmazonS3Client(accessKeyId, secretAccessKeyId, config);
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

            string basePath = (BasePath ?? String.Empty).Trim();
            string prefix = (Prefix ?? String.Empty).Trim();
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
                key = Urls.Combine(prefix, key);
            }

            return key;
        }

        /// <summary>
        /// Publishes the currently-identified file set to Amazon S3.
        /// </summary>
        public void Publish()
        {
            if (String.IsNullOrEmpty(BucketName))
            {
                throw new InvalidOperationException("BucketName must have a value.");
            }

            foreach (string filePath in Files)
            {
                PublishFile(filePath);
            }
        }

        /// <summary>
        /// Publishes a file to Amazon S3.
        /// </summary>
        /// <param name="filePath">The path of the file to publish.</param>
        private void PublishFile(string filePath)
        {
            NameValueCollection headers = new NameValueCollection();
            string contentType = MimeType.FromCommon(filePath).ContentType;
            string objectKey = ObjectKey(filePath);

            PutObjectRequest request = new PutObjectRequest()
                .WithBucketName(BucketName)
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

            using (PutObjectResponse response = Client().PutObject(request)) { }

            if (!String.IsNullOrEmpty(tempPath) && File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            PublisherDelegate.OnFilePublished(filePath, objectKey, gzip);
        }

        /// <summary>
        /// Sets the value of <see cref="BasePath"/> and returns this instance.
        /// </summary>
        /// <param name="basePath">The value to set.</param>
        /// <returns>This instance.</returns>
        public S3Publisher WithBasePath(string basePath)
        {
            BasePath = basePath;
            return this;
        }

        /// <summary>
        /// Sets the value of <see cref="BucketName"/> and returns this instance.
        /// </summary>
        /// <param name="bucketName">The value to set.</param>
        /// <returns>This instance.</returns>
        public S3Publisher WithBucketName(string bucketName)
        {
            BucketName = bucketName;
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
        /// Sets the value of <see cref="Prefix"/> and returns this instance.
        /// </summary>
        /// <param name="prefix">The value to set.</param>
        /// <returns>This instance.</returns>
        public S3Publisher WithPrefix(string prefix)
        {
            Prefix = prefix;
            return this;
        }

        /// <summary>
        /// Sets the value of <see cref="PublisherDelegate"/> and returns this instance.
        /// </summary>
        /// <param name="publisherDelegate">The value to set.</param>
        /// <returns>This instance.</returns>
        public S3Publisher WithPublisherDelegate(IS3PublisherDelegate publisherDelegate)
        {
            PublisherDelegate = publisherDelegate;
            return this;
        }

        /// <summary>
        /// Sets the value of <see cref="UseSsl"/> and returns this instance.
        /// </summary>
        /// <param name="useSsl">The value to set.</param>
        /// <returns>This instance.</returns>
        public S3Publisher WithUseSsl(bool useSsl)
        {
            UseSsl = useSsl;
            return this;
        }

        #endregion

        #region IS3PublisherDelegate Members

        /// <summary>
        /// Called when a file has been successfully published to Amazon S3.
        /// </summary>
        /// <param name="path">The path of the file that was published.</param>
        /// <param name="objectKey">The resulting object key of the file on Amazon S3.</param>
        /// <param name="withGzip">A value indicating whether the file was compressed with GZip before publishing.</param>
        public void OnFilePublished(string path, string objectKey, bool withGzip) { }

        #endregion
    }
}
