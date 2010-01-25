using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Tasty;
using Tasty.Build;

namespace Tasty.Test
{
    [TestClass]
    public class S3PublisherTests
    {
        [TestMethod]
        public void S3Publisher_CanGetObjectKey()
        {
            string basePath = @"C:\Projects\Tasty\Web\assets";
            string filePath = @"C:\Projects\Tasty\Web\assets\script\my-script.js";
            string prefix = "assets";

            Assert.AreEqual("assets/script/my-script.js", S3Publisher.GetObjectKey(prefix, basePath, filePath));
        }

        [TestMethod]
        public void S3Publisher_CanPublishDirectory()
        {
            string accessKeyId = ConfigurationManager.AppSettings["S3AccessKeyId"];
            string secretAccessKeyId = ConfigurationManager.AppSettings["S3SecretAccessKeyId"];
            string bucketName = ConfigurationManager.AppSettings["TestS3BucketName"];
            string prefix = DateTime.UtcNow.ToIso8601UtcPathSafeString();

            var publisher = new S3Publisher(accessKeyId, secretAccessKeyId)
            {
                BucketName = bucketName,
                DirectoryPath = Environment.CurrentDirectory,
                Prefix = prefix
            };

            publisher.Publish();

            AmazonS3 client = AWSClientFactory.CreateAmazonS3Client(accessKeyId, secretAccessKeyId, new AmazonS3Config() { CommunicationProtocol = Protocol.HTTP });

            GetObjectMetadataRequest request = new GetObjectMetadataRequest()
                .WithBucketName(bucketName)
                .WithKey(S3Publisher.GetObjectKey(prefix, Environment.CurrentDirectory, Path.Combine(Environment.CurrentDirectory, @"css\yui-fonts-min.css")));

            using (GetObjectMetadataResponse response = client.GetObjectMetadata(request))
            {
                Assert.AreEqual("text/css", response.ContentType);
                Assert.AreEqual("gzip", response.Headers["Content-Encoding"]);
            }
        }
    }
}
