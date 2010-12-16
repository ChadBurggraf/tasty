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
    public class S3PublisherTests : S3TestBase, IS3PublisherDelegate
    {
        private static List<string> cleanupKeys = new List<string>();
        private bool assertPublished, skipFileDelegateCalled, skipPrefixDelegateCalled;

        [ClassCleanup]
        public static void Cleanup()
        {
            foreach (string key in cleanupKeys)
            {
                try
                {
                    DeleteObjectRequest request = new DeleteObjectRequest()
                        .WithBucketName(BucketName)
                        .WithKey(key);

                    using (DeleteObjectResponse response = Client.DeleteObject(request))
                    {
                    }
                }
                catch
                {
                }
            }
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            cleanupKeys.Clear();
        }

        [TestMethod]
        public void S3Publisher_Publisher()
        {
            this.assertPublished = true;
            string prefix = DateTime.UtcNow.ToIso8601UtcPathSafeString();
            string[] files = new[]
            {
                @"css\yui-fonts-min.css",
                @"css\yui-reset-min.css",
                @"images\accept.png",
                @"images\add.png",
                @"images\anchor.png",
                @"script\jquery-ui.min.js",
                @"script\jquery.min.js"
            };

            new S3Publisher(AccessKeyId, SecretAccessKeyId)
                .WithBasePath(@".\")
                .WithBucketName(BucketName)
                .WithFiles(files)
                .WithPrefix(prefix)
                .WithPublisherDelegate(this) // Asserts are happening in the delegate.
                .WithUseSsl(UseSsl)
                .Publish();

            cleanupKeys.AddRange(from f in files
                                 let k = f.Replace(@"\", "/")
                                 select prefix + "/" + k);
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

            var publisher = new S3Publisher(AccessKeyId, SecretAccessKeyId)
                .WithBasePath(@".\")
                .WithBucketName(BucketName)
                .WithFiles(new string[] { @"css\yui-fonts-min.css" })
                .WithOverwriteExisting(false)
                .WithPrefix(prefix)
                .WithPublisherDelegate(this) // Asserts are happening in the delegate.
                .WithUseSsl(UseSsl);

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

            var publisher = new S3Publisher(AccessKeyId, SecretAccessKeyId)
                .WithBasePath(@".\")
                .WithBucketName(BucketName)
                .WithFiles(new string[] { @"css\yui-fonts-min.css" })
                .WithOverwriteExistingPrefix(false)
                .WithPrefix(prefix)
                .WithPublisherDelegate(this) // Asserts are happening in the delegate.
                .WithUseSsl(UseSsl);

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
                    .WithBucketName(BucketName)
                    .WithKey(objectKey);

                using (GetObjectMetadataResponse response = Client.GetObjectMetadata(request))
                {
                    Assert.AreEqual(MimeType.FromCommon(objectKey).ContentType, response.ContentType);

                    if (withGzip)
                    {
                        Assert.AreEqual("gzip", response.Headers["Content-Encoding"]);
                    }
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
