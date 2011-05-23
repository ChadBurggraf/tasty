//-----------------------------------------------------------------------
// <copyright file="S3PublisherTests.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Amazon.S3.Model;
    using Microsoft.Build.BuildEngine;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tasty;
    using Tasty.Build;
    using Tasty.Web;

    /// <summary>
    /// S3 publisher tests.
    /// </summary>
    [TestClass]
    public class S3PublisherTests : S3TestBase, IS3PublisherDelegate
    {
        private static List<string> cleanupKeys = new List<string>();
        private bool assertPublished, skipFileDelegateCalled, skipPrefixDelegateCalled;

        /// <summary>
        /// Cleans up any keys published to S3 during testing.
        /// </summary>
        [ClassCleanup]
        public static void Cleanup()
        {
            foreach (string key in cleanupKeys)
            {
                DeleteObjectRequest request = new DeleteObjectRequest()
                        .WithBucketName(BucketName)
                        .WithKey(key);

                using (DeleteObjectResponse response = Client.DeleteObject(request))
                {
                }
            }
        }

        /// <summary>
        /// Initializes the class prior to a test run.
        /// </summary>
        /// <param name="context">The test context to initialize with.</param>
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            cleanupKeys.Clear();
        }

        /// <summary>
        /// Publish tests.
        /// </summary>
        [TestMethod]
        public void S3PublisherPublish()
        {
            if (!IsEmpty)
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
        }

        /// <summary>
        /// Publish MSBuild tests.
        /// </summary>
        [TestMethod]
        public void S3PublisherPublishMSBuild()
        {
            if (!IsEmpty)
            {
                const string ProjectFile = "S3Publish.proj";

                if (File.Exists(ProjectFile))
                {
                    Assert.IsTrue(Engine.GlobalEngine.BuildProjectFile(ProjectFile));
                }
            }
        }

        /// <summary>
        /// Skip existing files tests.
        /// </summary>
        [TestMethod]
        public void S3PublisherSkipExistingFile()
        {
            if (!IsEmpty)
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
        }

        /// <summary>
        /// Skip existing prefix tests.
        /// </summary>
        [TestMethod]
        public void S3PublisherSkipExistingPrefix()
        {
            if (!IsEmpty)
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
        }

        #region IS3PublisherDelegate Members

        /// <summary>
        /// Invoked when the publisher has published a file.
        /// </summary>
        /// <param name="path">The local path of the file that was published.</param>
        /// <param name="objectKey">The object key the file was published to.</param>
        /// <param name="withGzip">A value indicating whether the file was GZipped prior to publishing.</param>
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

        /// <summary>
        /// Invoked when the publisher has skipped a file because it already exists.
        /// </summary>
        /// <param name="path">The local path of the file that was skipped.</param>
        /// <param name="objectKey">The object key of the existing object.</param>
        public void OnFileSkipped(string path, string objectKey)
        {
            this.skipFileDelegateCalled = true;
        }

        /// <summary>
        /// Invoked when the publisher has skipped an entire prefix because it already exists.
        /// </summary>
        /// <param name="prefix">The prefix that was skipped.</param>
        public void OnPrefixSkipped(string prefix)
        {
            this.skipPrefixDelegateCalled = true;
        }

        #endregion
    }
}
