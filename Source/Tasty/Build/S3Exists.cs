//-----------------------------------------------------------------------
// <copyright file="S3KeyExists.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Build
{
    using System;
    using System.Net;
    using Amazon;
    using Amazon.S3;
    using Amazon.S3.Model;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// Base class for S3-exists related <see cref="Task"/>s.
    /// </summary>
    public abstract class S3Exists : Task
    {
        private AmazonS3 client;

        /// <summary>
        /// Gets or sets the Amazon S3 access key ID to use when connecting to the service.
        /// </summary>
        [Required]
        public string AccessKeyId { get; set; }

        /// <summary>
        /// Gets or sets the name of the bucket to publish assets to.
        /// </summary>
        [Required]
        public string BucketName { get; set; }

        /// <summary>
        /// Gets the client to use when connecting to the service.
        /// </summary>
        protected AmazonS3 Client
        {
            get
            {
                if (this.client == null)
                {
                    AmazonS3Config config = new AmazonS3Config();
                    config.CommunicationProtocol = this.UseSsl ? Protocol.HTTPS : Protocol.HTTP;
                    this.client = AWSClientFactory.CreateAmazonS3Client(this.AccessKeyId, this.SecretAccessKeyId, config);
                }

                return this.client;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the item exists.
        /// </summary>
        [Output]
        public bool Exists { get; set; }

        /// <summary>
        /// Gets or sets the Amazon S3 secret access key ID to use when connecting to the service.
        /// </summary>
        [Required]
        public string SecretAccessKeyId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use SSL when connecting to the service.
        /// </summary>
        public bool UseSsl { get; set; }
    }
}
