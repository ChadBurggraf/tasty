//-----------------------------------------------------------------------
// <copyright file="S3PrefixExists.cs" company="Tasty Codes">
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
    /// Checks for the existence of a key prefix in an Amazon S3 bucket.
    /// </summary>
    public class S3PrefixExists : S3Exists
    {
        /// <summary>
        /// Gets or sets the key prefix to check for the existstence of.
        /// </summary>
        [Required]
        public string Prefix { get; set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>True if the task executed successfully, false otherwise.</returns>
        public override bool Execute()
        {
            ListObjectsRequest request = new ListObjectsRequest()
                    .WithBucketName(BucketName)
                    .WithPrefix(this.Prefix)
                    .WithMaxKeys(1);

            using (ListObjectsResponse response = Client.ListObjects(request))
            {
                Exists = response.S3Objects != null && response.S3Objects.Count > 0;
            }

            return true;
        }
    }
}
