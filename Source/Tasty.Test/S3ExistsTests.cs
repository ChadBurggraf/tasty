//-----------------------------------------------------------------------
// <copyright file="S3ExistsTests.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.Collections.Generic;
    using Amazon.S3.Model;
    using Microsoft.Build.BuildEngine;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tasty.Build;

    /// <summary>
    /// S3 exists tests.
    /// </summary>
    [TestClass]
    public class S3ExistsTests : S3TestBase
    {
        private static List<string> cleanupKeys = new List<string>();

        /// <summary>
        /// Cleans up object keys published to S3 during testing.
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
        /// Key does not exist tests.
        /// </summary>
        [TestMethod]
        public void S3ExistsKeyDoesNotExist()
        {
            if (!IsEmpty)
            {
                S3KeyExists task = new S3KeyExists()
                {
                    AccessKeyId = AccessKeyId,
                    BucketName = BucketName,
                    Key = Guid.NewGuid().ToString(),
                    SecretAccessKeyId = SecretAccessKeyId,
                    UseSsl = UseSsl
                };

                task.Execute();
                Assert.IsFalse(task.Exists);
            }
        }

        /// <summary>
        /// Key exists tests.
        /// </summary>
        [TestMethod]
        public void S3ExistsKeyExists()
        {
            if (!IsEmpty)
            {
                string key = Guid.NewGuid().ToString() + ".js";

                PutObjectRequest request = new PutObjectRequest()
                    .WithBucketName(BucketName)
                    .WithKey(key)
                    .WithFilePath(@"script\jquery.min.js");

                using (PutObjectResponse response = Client.PutObject(request))
                {
                    cleanupKeys.Add(key);
                }

                S3KeyExists task = new S3KeyExists()
                {
                    AccessKeyId = AccessKeyId,
                    BucketName = BucketName,
                    Key = key,
                    SecretAccessKeyId = SecretAccessKeyId,
                    UseSsl = UseSsl
                };

                task.Execute();
                Assert.IsTrue(task.Exists);
            }
        }

        /// <summary>
        /// Key project file tests.
        /// </summary>
        [TestMethod]
        public void S3ExistsKeyProjectFile()
        {
            if (!IsEmpty)
            {
                string key = Guid.NewGuid().ToString() + ".js";

                Project project = new Project(Engine.GlobalEngine);
                project.Load("S3Exists.proj");

                BuildPropertyGroup properties = project.AddNewPropertyGroup(false);
                properties.AddNewProperty("Key", key);
                properties.AddNewProperty("S3AccessKeyId", AccessKeyId);
                properties.AddNewProperty("S3BucketName", BucketName);
                properties.AddNewProperty("S3SecretAccessKeyId", SecretAccessKeyId);
                properties.AddNewProperty("S3UseSsl", UseSsl.ToString());

                project.Build("KeyExists");

                PutObjectRequest request = new PutObjectRequest()
                    .WithBucketName(BucketName)
                    .WithKey(key)
                    .WithFilePath(@"script\jquery.min.js");

                using (PutObjectResponse response = Client.PutObject(request))
                {
                    cleanupKeys.Add(key);
                }

                project.Build("KeyExists");
            }
        }

        /// <summary>
        /// Prefix does not exists tests.
        /// </summary>
        [TestMethod]
        public void S3ExistsPrefixDoesNotExist()
        {
            if (!IsEmpty)
            {
                S3PrefixExists task = new S3PrefixExists()
                {
                    AccessKeyId = AccessKeyId,
                    BucketName = BucketName,
                    Prefix = Guid.NewGuid().ToString(),
                    SecretAccessKeyId = SecretAccessKeyId,
                    UseSsl = UseSsl
                };

                task.Execute();
                Assert.IsFalse(task.Exists);
            }
        }

        /// <summary>
        /// Prefix exists tests.
        /// </summary>
        [TestMethod]
        public void S3ExistsPrefixExists()
        {
            if (!IsEmpty)
            {
                string prefix = Guid.NewGuid().ToString();
                string key = String.Concat(prefix, "/jquery-min.js");

                PutObjectRequest request = new PutObjectRequest()
                    .WithBucketName(BucketName)
                    .WithKey(key)
                    .WithFilePath(@"script\jquery.min.js");

                using (PutObjectResponse response = Client.PutObject(request))
                {
                    cleanupKeys.Add(key);
                }

                S3PrefixExists task = new S3PrefixExists()
                {
                    AccessKeyId = AccessKeyId,
                    BucketName = BucketName,
                    Prefix = prefix,
                    SecretAccessKeyId = SecretAccessKeyId,
                    UseSsl = UseSsl
                };

                task.Execute();
                Assert.IsTrue(task.Exists);
            }
        }

        /// <summary>
        /// Prefix project file tests.
        /// </summary>
        [TestMethod]
        public void S3ExistsPrefixProjectFile()
        {
            if (!IsEmpty)
            {
                string prefix = Guid.NewGuid().ToString();
                string key = prefix + "/jquery.min.js";

                Project project = new Project(Engine.GlobalEngine);
                project.Load("S3Exists.proj");

                BuildPropertyGroup properties = project.AddNewPropertyGroup(false);
                properties.AddNewProperty("Prefix", prefix);
                properties.AddNewProperty("S3AccessKeyId", AccessKeyId);
                properties.AddNewProperty("S3BucketName", BucketName);
                properties.AddNewProperty("S3SecretAccessKeyId", SecretAccessKeyId);
                properties.AddNewProperty("S3UseSsl", UseSsl.ToString());

                project.Build("PrefixExists");

                PutObjectRequest request = new PutObjectRequest()
                    .WithBucketName(BucketName)
                    .WithKey(key)
                    .WithFilePath(@"script\jquery.min.js");

                using (PutObjectResponse response = Client.PutObject(request))
                {
                    cleanupKeys.Add(key);
                }

                project.Build("PrefixExists");
            }
        }
    }
}
