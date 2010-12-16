//-----------------------------------------------------------------------
// <copyright file="S3KeyExists.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Build
{
    using System;
    using System.Net;
    using Amazon.S3;
    using Amazon.S3.Model;
    using Microsoft.Build.Framework;

    /// <summary>
    /// Checks for the existence of an object key in an Amazon S3 bucket.
    /// </summary>
    public class S3KeyExists : S3Exists
    {
        /// <summary>
        /// Gets or sets the object key to check for the existstence of.
        /// </summary>
        [Required]
        public string Key { get; set; }
        
        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>True if the task executed successfully, false otherwise.</returns>
        public override bool Execute()
        {
            bool exists = false;

            GetObjectMetadataRequest request = new GetObjectMetadataRequest()
                .WithBucketName(BucketName)
                .WithKey(this.Key);

            try
            {
                using (GetObjectMetadataResponse response = Client.GetObjectMetadata(request))
                {
                    exists = true;
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound || !"NoSuchKey".Equals(ex.ErrorCode, StringComparison.OrdinalIgnoreCase))
                {
                    throw;
                }
            }

            Exists = exists;
            return true;
        }
    }
}
