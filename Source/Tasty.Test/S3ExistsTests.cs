using System;
using System.Collections.Generic;
using Amazon.S3.Model;
using Microsoft.Build.BuildEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Build;

namespace Tasty.Test
{
    [TestClass]
    public class S3ExistsTests : S3TestBase
    {
        private static List<string> cleanupKeys = new List<string>();

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
        public void S3Exists_KeyDoesNotExist()
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

        [TestMethod]
        public void S3Exists_KeyExists()
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

        [TestMethod]
        public void S3Exists_KeyProjectFile()
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

        [TestMethod]
        public void S3Exists_PrefixDoesNotExist()
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

        [TestMethod]
        public void S3Exists_PrefixExists()
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

        [TestMethod]
        public void S3Exists_PrefixProjectFile()
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
