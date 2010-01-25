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
using DotZLib;

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
        private IList<string> fileTypes;
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
        /// Gets or sets the bucket name to use when publishing files.
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// Gets or sets the path to the directory to publish.
        /// </summary>
        public string DirectoryPath { get; set; }

        /// <summary>
        /// Gets a collection of file types to filter on
        /// when publishing the directory.
        /// </summary>
        public IList<string> FileTypes
        {
            get
            {
                if (fileTypes == null)
                {
                    fileTypes = new List<string>(new string[] {
                        ".css",
                        ".gif",
                        ".jpg",
                        ".js",
                        ".png"
                    });
                }

                return fileTypes;
            }
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
        /// Publishes the currently-identified directory to Amazon S3.
        /// </summary>
        public void Publish()
        {
            if (String.IsNullOrEmpty(BucketName))
            {
                throw new InvalidOperationException("BucketName must have a value.");
            }

            if (String.IsNullOrEmpty(DirectoryPath))
            {
                throw new InvalidOperationException("DirectoryPath must have a value.");
            }

            PublishDirectory(
                accessKeyId, 
                secretAccessKeyId, 
                BucketName, 
                Prefix, 
                UseSsl, 
                DirectoryPath, 
                GetSearchPatternFromFileTypes(FileTypes), 
                PublisherDelegate
            );
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

        #region Static Methods

        /// <summary>
        /// Gets the object key to use for the file at the given path.
        /// Uses basePath to expand prefix and maintain the file's folder hierarchy on S3.
        /// </summary>
        /// <param name="prefix">The prefix which represents the existing S3 hierarchy to place the object in.</param>
        /// <param name="basePath">The base path on the file system which represents the given prefix's root.</param>
        /// <param name="filePath">The path to the file that is being published to S3.</param>
        /// <returns>A file's S3 object key.</returns>
        public static string GetObjectKey(string prefix, string basePath, string filePath)
        {
            string key = filePath.Substring(basePath.Length).Replace(@"\", "/");

            if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                key = Urls.Combine(prefix, key);
            }

            return key;
        }

        /// <summary>
        /// Gets the content type to use when publishing the file at the given path.
        /// </summary>
        /// <param name="path">The path of the file being published.</param>
        /// <returns>The file's content type.</returns>
        /// <exception cref="System.ArgumentException">Thrown if the path does not point to a known file type.</exception>
        public static string GetPublishContentType(string path)
        {
            string ext = Path.GetExtension(path).ToUpperInvariant();

            switch (ext)
            {
                case (".JS"):
                    return "text/javascript";
                case (".CSS"):
                    return "text/css";
                case (".JPG"):
                case (".JPEG"):
                    return "image/jpeg";
                case (".GIF"):
                    return "image/gif";
                case (".PNG"):
                    return "image/png";
                default:
                    throw new ArgumentException("The given path does not point to a publish-able content type.", "path");
            }
        }

        /// <summary>
        /// Gets a search pattern that represents the given file type collection.
        /// </summary>
        /// <param name="fileTypes">A file type collection to get a search pattern for.</param>
        /// <returns>A file search pattern.</returns>
        private static string GetSearchPatternFromFileTypes(IList<string> fileTypes)
        {
            StringBuilder sb = new StringBuilder();

            if (fileTypes != null && fileTypes.Count > 0)
            {
                for (int i = 0; i < fileTypes.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(";");
                    }

                    string fileType = fileTypes[i];

                    if (String.IsNullOrEmpty(fileType) ||
                        !fileType.StartsWith(".", StringComparison.Ordinal) ||
                        fileType.Contains(";"))
                    {
                        throw new ArgumentException("A search file type must not be empty, must be a file extension (e.g., .png) and must not contain a semi-colon.");
                    }

                    sb.Append("*");
                    sb.Append(fileType);
                }

                return sb.ToString();
            }
            else
            {
                sb.Append("*.*");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Publishes a directory, recursively, to Amazon S3.
        /// </summary>
        /// <param name="accessKeyId">The Amazon access key ID to use when connecting to the service.</param>
        /// <param name="secretAccessKeyId">The Amazon secret access key ID to use when connecting to the service.</param>
        /// <param name="bucketName">The name of the bucket to publish to.</param>
        /// <param name="prefix">The path prefix to use for the published files (sub-directories will be appended to this prefix to maintain the filesystem hierarchy).</param>
        /// <param name="useSsl">A value indicating whether to use SSL when connecting to the service.</param>
        /// <param name="path">The path of the directory to publish.</param>
        /// <param name="searchPattern">The search pattern to use for filtering on file types. Multiple search patterns can be separated by semi-colons.</param>
        /// <param name="publisherDelegate">The delegate that should receive publish notifications.</param>
        private static void PublishDirectory(string accessKeyId, string secretAccessKeyId, string bucketName, string prefix, bool useSsl, string path, string searchPattern, IS3PublisherDelegate publisherDelegate)
        {
            prefix = (prefix ?? String.Empty).Trim();
            string[] files = Files.GetFilesForPatterns(path, searchPattern);

            AmazonS3Config config = new AmazonS3Config();
            config.CommunicationProtocol = useSsl ? Protocol.HTTPS : Protocol.HTTP;
            AmazonS3 client = AWSClientFactory.CreateAmazonS3Client(accessKeyId, secretAccessKeyId, config);

            foreach (string filePath in files)
            {
                NameValueCollection headers = new NameValueCollection();
                string contentType = GetPublishContentType(filePath);
                string objectKey = GetObjectKey(prefix, path, filePath);
                bool gzip = false;
                string tempPath = null;

                PutObjectRequest request = new PutObjectRequest()
                    .WithBucketName(bucketName)
                    .WithCannedACL(S3CannedACL.PublicRead)
                    .WithContentType(contentType)
                    .WithKey(objectKey);

                if (contentType.StartsWith("text", StringComparison.OrdinalIgnoreCase))
                {
                    gzip = true;
                    tempPath = Path.GetTempFileName();

                    headers["Content-Encoding"] = "gzip";

                    using (GZipStream gz = new GZipStream(tempPath, CompressLevel.Default))
                    {
                        using (FileStream fs = File.OpenRead(filePath))
                        {
                            byte[] buffer = new byte[4096];
                            int count = 0;

                            while (0 < (count = fs.Read(buffer, 0, buffer.Length)))
                            {
                                gz.Write(buffer, 0, count);
                            }
                        }
                    }

                    request = request.WithFilePath(tempPath);
                }
                else
                {
                    request = request.WithFilePath(filePath);
                }

                request.AddHeaders(headers);

                using (PutObjectResponse response = client.PutObject(request)) { }

                if (!String.IsNullOrEmpty(tempPath) && File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                publisherDelegate.OnFilePublished(filePath, objectKey, gzip);
            }

            string[] directories = Directory.GetDirectories(path);

            foreach (string directory in directories)
            {
                string name = directory.Substring(path.Length);

                if (name.StartsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    name = name.Substring(1);
                }

                PublishDirectory(accessKeyId, secretAccessKeyId, bucketName, Urls.Combine(prefix, name), useSsl, directory, searchPattern, publisherDelegate);
            }
        }

        #endregion
    }
}
