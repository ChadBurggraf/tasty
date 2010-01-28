using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace Tasty.Web
{
    /// <summary>
    /// Represents a MIME type consisting of a content-type and a set of file extensions.
    /// </summary>
    public class MimeType
    {
        #region Construction

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="contentType">The MIME type's content type.</param>
        public MimeType(string contentType)
            : this(contentType, new string[0]) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="contentType">The MIME type's content type.</param>
        /// <param name="extensions">The MIME type's valid extension set.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Lowercase is preferred for content types.")]
        public MimeType(string contentType, IEnumerable<string> extensions)
        {
            ContentType = contentType.ToLowerInvariant();
            Extensions = new ReadOnlyCollection<string>((from ext in extensions
                                                         select GetNormalizedExtension(ext)).ToArray());
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the MIME type's content type.
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// Gets the MIME type's valid extension set.
        /// </summary>
        public ReadOnlyCollection<string> Extensions { get; private set; }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Adds a new extension to this MIME type's valid extension set.
        /// </summary>
        /// <param name="extension">The extension to add.</param>
        public void AddExtension(string extension)
        {
            extension = GetNormalizedExtension(extension);

            if (!Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                List<string> temp = new List<string>(Extensions);
                temp.Add(extension);

                Extensions = new ReadOnlyCollection<string>(temp);
            }
        }

        /// <summary>
        /// Adds a new collection of extensions to this MIME type's valid extension set.
        /// </summary>
        /// <param name="extensions">The set of extensions to add.</param>
        public void AddExtensions(IEnumerable<string> extensions)
        {
            List<string> temp = new List<string>(Extensions);

            var exts = from ext in extensions
                       select GetNormalizedExtension(ext);

            exts = (from ext in exts
                    where !temp.Contains(ext)
                    select ext).ToArray();

            temp.AddRange(exts);

            Extensions = new ReadOnlyCollection<string>(temp);
        }

        /// <summary>
        /// Gets a value indicating whether the given extension is valid for this MIME type.
        /// </summary>
        /// <param name="extension">The extension to check.</param>
        /// <returns>True if the extension is valid, false otherwise.</returns>
        public bool ContainsExtension(string extension)
        {
            return Extensions.Contains(GetNormalizedExtension(extension), StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets a common internet MIME type from the given path's file extension.
        /// </summary>
        /// <param name="path">The path of the file to get the common MIME type for.</param>
        /// <returns>A common MIME type, or "application/octet-stream" if the extension was not recognized.</returns>
        public static MimeType FromCommon(string path)
        {
            string ext = Path.GetExtension(path).ToUpperInvariant();
            string[] extensions;
            string contentType;

            switch (ext)
            {
                case (".CSS"):
                    extensions = new string[] { ".css" };
                    contentType = "text/css";
                    break;
                case (".CSV"):
                    extensions = new string[] { ".csv" };
                    contentType = "text/csv";
                    break;
                case (".HTML"):
                case (".HTM"):
                    extensions = new string[] { ".html", ".htm" };
                    contentType = "text/html";
                    break;
                case (".JS"):
                    extensions = new string[] { ".js" };
                    contentType = "text/javascript";
                    break;
                case (".TXT"):
                    extensions = new string[] { ".txt" };
                    contentType = "text/plain";
                    break;
                case (".XML"):
                    extensions = new string[] { ".xml" };
                    contentType = "text/xml";
                    break;
                case (".GIF"):
                    extensions = new string[] { ".gif" };
                    contentType = "image/gif";
                    break;
                case (".JPG"):
                case (".JPEG"):
                    extensions = new string[] { ".jpg", ".jpeg" };
                    contentType = "image/jpeg";
                    break;
                case (".PNG"):
                    extensions = new string[] { ".png" };
                    contentType = "image/png";
                    break;
                case (".SVG"):
                    extensions = new string[] { ".svg" };
                    contentType = "image/svg+xml";
                    break;
                case (".TIFF"):
                    extensions = new string[] { ".tiff" };
                    contentType = "image/tiff";
                    break;
                case (".ICO"):
                    extensions = new string[] { ".ico" };
                    contentType = "image/vnd.microsoft.icon";
                    break;
                case (".M4A"):
                    extensions = new string[] { ".m4a" };
                    contentType = "audio/mp4";
                    break;
                case (".MP3"):
                    extensions = new string[] { ".mp3" };
                    contentType = "audio/mpeg";
                    break;
                case (".OGG"):
                case (".OGA"):
                    extensions = new string[] { ".ogg", ".oga" };
                    contentType = "audio/ogg";
                    break;
                case (".WMA"):
                    extensions = new string[] { ".wma" };
                    contentType = "audio/x-ms-wma";
                    break;
                case (".RA"):
                case (".RAM"):
                    extensions = new string[] { ".ra", ".ram" };
                    contentType = "audio/vnd.rn-realaudio";
                    break;
                case (".WAV"):
                    extensions = new string[] { ".wav" };
                    contentType = "audio/vnd.wave";
                    break;
                case (".MPG"):
                case (".MPEG"):
                    extensions = new string[] { ".mpg", ".mpeg" };
                    contentType = "video/mpeg";
                    break;
                case (".MP4"):
                    extensions = new string[] { ".mp4" };
                    contentType = "video/mp4";
                    break;
                case (".OGV"):
                    extensions = new string[] { ".ogv" };
                    contentType = "video/ogg";
                    break;
                case (".MOV"):
                case (".QT"):
                    extensions = new string[] { ".mov", ".qt" };
                    contentType = "video/quicktime";
                    break;
                case (".WMV"):
                    extensions = new string[] { ".wmv" };
                    contentType = "video/x-ms-wmv";
                    break;
                default:
                    extensions = new string[0];
                    contentType = "application/octet-stream";
                    break;
            }

            return new MimeType(contentType, extensions);
        }

        /// <summary>
        /// Gets a normalized extension string (lowercase, beginning with a period).
        /// </summary>
        /// <param name="extension">The extension to normalize.</param>
        /// <returns>The normalized extension string.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Lowercase is preferred for file extensions.")]
        private static string GetNormalizedExtension(string extension)
        {
            if (!extension.StartsWith(".", StringComparison.Ordinal))
            {
                extension = String.Concat(".", extension);
            }

            return extension.ToLowerInvariant();
        }

        /// <summary>
        /// Gets a value indicating whether the given content type is
        /// a valid MIME type for the given whitelist.
        /// </summary>
        /// <param name="allowed">A collection of allowed MIME types to check.</param>
        /// <param name="contentType">The content type to check.</param>
        /// <returns>True if the MIME type is valid, false otherwise.</returns>
        public static bool IsValidMimeType(IEnumerable<MimeType> allowed, string contentType)
        {
            return 0 < (from m in allowed
                        where m.ContentType.Equals(contentType, StringComparison.OrdinalIgnoreCase)
                        select m).Count();
        }

        /// <summary>
        /// Gets a value indicating whether the given content type and file name constitute
        /// a valid MIME type for the given whitelist.
        /// </summary>
        /// <param name="allowed">A collection of allowed MIME types to check.</param>
        /// <param name="contentType">The content type to check.</param>
        /// <param name="fileName">The file name to check.</param>
        /// <returns>True if the MIME type is valid, false otherwise.</returns>
        public static bool IsValidMimeType(IEnumerable<MimeType> allowed, string contentType, string fileName)
        {
            return 0 < (from m in allowed
                        where m.ContentType.Equals(contentType, StringComparison.OrdinalIgnoreCase) && m.ContainsExtension(Path.GetExtension(fileName))
                        select m).Count();
        }

        #endregion
    }
}
