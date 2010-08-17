//-----------------------------------------------------------------------
// <copyright file="IS3PublisherDelegate.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Build
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Delegate for notifications during an <see cref="S3Publisher"/> publish process.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Well, it is a delegate isn't it?")]
    public interface IS3PublisherDelegate
    {
        /// <summary>
        /// Called when a file has been successfully published to Amazon S3.
        /// </summary>
        /// <param name="path">The path of the file that was published.</param>
        /// <param name="objectKey">The resulting object key of the file on Amazon S3.</param>
        /// <param name="withGzip">A value indicating whether the file was compressed with GZip before publishing.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        void OnFilePublished(string path, string objectKey, bool withGzip);

        /// <summary>
        /// Called when a file is skipped for publishing because it already exists.
        /// </summary>
        /// <param name="path">The path of the file that was skipped.</param>
        /// <param name="objectKey">The object key of the file on the service.</param>
        void OnFileSkipped(string path, string objectKey);

        /// <summary>
        /// Called when an entire prefix is skipped for publishing because it already exists.
        /// </summary>
        /// <param name="prefix">The prefix that was skipped.</param>
        void OnPrefixSkipped(string prefix);
    }
}
