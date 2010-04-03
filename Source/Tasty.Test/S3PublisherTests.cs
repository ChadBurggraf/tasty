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

        static S3PublisherTests()
        {
            AmazonS3Config config = new AmazonS3Config() { CommunicationProtocol = Protocol.HTTP };
            s3Client = AWSClientFactory.CreateAmazonS3Client(accessKeyId, secretAccessKeyId, config);
        }

        [TestMethod]
        public void S3Publisher_Publisher()
        {
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
                .WithUseSsl(false).Publish();
        }

        [TestMethod]
        public void S3Publisher_PublishMsBuild()
        {
            Assert.IsTrue(Engine.GlobalEngine.BuildProjectFile("S3Publish.proj"));
        }

        #region IS3PublisherDelegate Members

        public void OnFilePublished(string path, string objectKey, bool withGzip)
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

        #endregion
    }
}
