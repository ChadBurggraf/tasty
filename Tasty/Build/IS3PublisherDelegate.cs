using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Tasty.Build
{
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
    }
}
