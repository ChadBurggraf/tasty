//-----------------------------------------------------------------------
// <copyright file="S3TestBase.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using Amazon;
    using Amazon.S3;
    using Amazon.S3.Model;

    /// <summary>
    /// Serves as the base class for S3-related tests.
    /// </summary>
    public abstract class S3TestBase
    {
        private static string accessKeyId, bucketName, secretAccessKeyId;
        private static bool? useSsl;
        private static AmazonS3 client;

        /// <summary>
        /// Gets the access key to use to access the service.
        /// </summary>
        protected static string AccessKeyId
        {
            get { return accessKeyId ?? (accessKeyId = ConfigurationManager.AppSettings["S3AccessKeyId"]); }
        }

        /// <summary>
        /// Gets the bucket name to use when accessing the service.
        /// </summary>
        protected static string BucketName
        {
            get { return bucketName ?? (bucketName = ConfigurationManager.AppSettings["S3BucketName"]); }
        }

        /// <summary>
        /// Gets the client to use when accessing S3.
        /// </summary>
        protected static AmazonS3 Client
        {
            get
            {
                if (client == null)
                {
                    AmazonS3Config config = new AmazonS3Config();
                    config.CommunicationProtocol = Convert.ToBoolean(ConfigurationManager.AppSettings["S3UseSsl"], CultureInfo.InvariantCulture) ? Protocol.HTTPS : Protocol.HTTP;
                    client = AWSClientFactory.CreateAmazonS3Client(AccessKeyId, SecretAccessKeyId, config);
                }

                return client;
            }
        }

        /// <summary>
        /// Gets a value indicating whether any of the S3 configuration values are empty.
        /// </summary>
        protected static bool IsEmpty
        {
            get { return String.IsNullOrEmpty(AccessKeyId) || String.IsNullOrEmpty(SecretAccessKeyId) || String.IsNullOrEmpty(BucketName); }
        }

        /// <summary>
        /// Gets the secret access key used to access the service.
        /// </summary>
        protected static string SecretAccessKeyId
        {
            get { return secretAccessKeyId ?? (secretAccessKeyId = ConfigurationManager.AppSettings["S3SecretAccessKeyId"]); }
        }

        /// <summary>
        /// Gets a value indicating whether to use SSL when connecting to the service.
        /// </summary>
        protected static bool UseSsl
        {
            get { return (bool)(useSsl ?? (useSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["S3UseSsl"], CultureInfo.InvariantCulture))); }
        }
    }
}
