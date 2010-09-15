using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.BuildEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Tasty;
using Tasty.Build;
using Tasty.Web;

namespace Tasty.Test
{
    [TestClass]
    public class S3PublisherTests : IS3PublisherDelegate
    {
        private static string accessKeyId = ConfigurationManager.AppSettings["S3AccessKeyId"];
        private static string secretAccessKeyId = ConfigurationManager.AppSettings["S3SecretAccessKeyId"];
        private static string bucketName = ConfigurationManager.AppSettings["S3BucketName"];
        private static AmazonS3 s3Client;
        private bool assertPublished, skipFileDelegateCalled, skipPrefixDelegateCalled;

        static S3PublisherTests()
        {
            AmazonS3Config config = new AmazonS3Config() { CommunicationProtocol = Protocol.HTTP };
            s3Client = AWSClientFactory.CreateAmazonS3Client(accessKeyId, secretAccessKeyId, config);
        }

        [TestMethod]
        public void S3Publisher_Publisher()
        {
            this.assertPublished = true;

            new S3Publisher(accessKeyId, secretAccessKeyId)
                .WithBasePath(@".\")
                .WithBucketName(bucketName)
                .WithFiles(new string[] {
                    @"css\yui-fonts-min.css",
                    @"css\yui-reset-min.css",
                    @"images\accept.png",
                    @"images\add.png",
                    @"images\anchor.png",
                    @"script\jquery-ui.min.js",
                    @"script\jquery.min.js"
                })
                .WithPrefix(DateTime.UtcNow.ToIso8601UtcPathSafeString())
                .WithPublisherDelegate(this) // Asserts are happening in the delegate.
                .WithUseSsl(false)
                .Publish();
        }

        [TestMethod]
        public void S3Publisher_PublishMsBuild()
        {
            const string ProjectFile = "S3Publish.proj";

            if (File.Exists(ProjectFile))
            {
                Assert.IsTrue(Engine.GlobalEngine.BuildProjectFile(ProjectFile));
            }
        }

        [TestMethod]
        public void S3Publisher_SkipExistingFile()
        {
            this.assertPublished = false;
            this.skipFileDelegateCalled = false;
            this.skipPrefixDelegateCalled = false;

            string prefix = DateTime.UtcNow.ToIso8601UtcPathSafeString();

            var publisher = new S3Publisher(accessKeyId, secretAccessKeyId)
                .WithBasePath(@".\")
                .WithBucketName(bucketName)
                .WithFiles(new string[] { @"css\yui-fonts-min.css" })
                .WithOverwriteExisting(false)
                .WithPrefix(prefix)
                .WithPublisherDelegate(this) // Asserts are happening in the delegate.
                .WithUseSsl(false);

            publisher.Publish();

            publisher
                .WithFiles(new string[] { @"css\yui-fonts-min.css", @"css\yui-reset-min.css" })
                .Publish();

            Assert.IsFalse(this.skipPrefixDelegateCalled);
            Assert.IsTrue(this.skipFileDelegateCalled);
        }

        [TestMethod]
        public void S3Publisher_SkipExistingPrefix()
        {
            this.assertPublished = false;
            this.skipPrefixDelegateCalled = false;

            string prefix = DateTime.UtcNow.ToIso8601UtcPathSafeString();

            var publisher = new S3Publisher(accessKeyId, secretAccessKeyId)
                .WithBasePath(@".\")
                .WithBucketName(bucketName)
                .WithFiles(new string[] { @"css\yui-fonts-min.css" })
                .WithOverwriteExistingPrefix(false)
                .WithPrefix(prefix)
                .WithPublisherDelegate(this) // Asserts are happening in the delegate.
                .WithUseSsl(false);

            publisher.Publish();
            publisher.Publish();

            Assert.IsTrue(this.skipPrefixDelegateCalled);
        }

        #region IS3PublisherDelegate Members

        public void OnFilePublished(string path, string objectKey, bool withGzip)
        {
            if (this.assertPublished)
            {
                GetObjectMetadataRequest request = new GetObjectMetadataRequest()
                    .WithBucketName(bucketName)
                    .WithKey(objectKey);

                using (GetObjectMetadataResponse response = s3Client.GetObjectMetadata(request))
                {
                    Assert.AreEqual(MimeType.FromCommon(objectKey).ContentType, response.ContentType);

                    if (withGzip)
                    {
                        Assert.AreEqual("gzip", response.Headers["Content-Encoding"]);
                    }
                }

                DeleteObjectRequest deleteRequest = new DeleteObjectRequest()
                    .WithBucketName(bucketName)
                    .WithKey(objectKey);

                using (DeleteObjectResponse deleteResponse = s3Client.DeleteObject(deleteRequest))
                {
                }
            }
        }

        public void OnFileSkipped(string path, string objectKey)
        {
            this.skipFileDelegateCalled = true;
        }

        public void OnPrefixSkipped(string prefix)
        {
            this.skipPrefixDelegateCalled = true;
        }

        #endregion
    }
}
