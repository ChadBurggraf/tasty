//-----------------------------------------------------------------------
// <copyright file="Files.cs" company="Chad Burggraf">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    /// <summary>
    /// Provides extensions for working with files.
    /// </summary>
    public static class Files
    {
        /// <summary>
        /// Gets a combined collection of file paths for each file found
        /// in the given directory for each of the given search patterns.
        /// </summary>
        /// <param name="path">The path of the directory to get files from.</param>
        /// <param name="searchPatterns">The search patterns to use, separated by a semi-colon.</param>
        /// <returns>A collection of file paths.</returns>
        public static string[] GetFilesForPatterns(string path, string searchPatterns)
        {
            List<string> results = new List<string>();
            string[] patterns = searchPatterns.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string pattern in patterns)
            {
                results.AddRange(Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly));
            }

            return results.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }

        /// <summary>
        /// Computes the SHA1 hash of the given stream. If the stream supports
        /// seeking it will be moved to position 0 before hashing if necessary.
        /// </summary>
        /// <param name="stream">The stream to compute the hash of.</param>
        /// <returns>The computed SHA1 hash.</returns>
        public static string Hash(this Stream stream)
        {
            if (stream.CanSeek && stream.Position != 0)
            {
                stream.Position = 0;
            }

            return SHA1Managed.Create().ComputeHash(stream).ToHex();
        }

        /// <summary>
        /// Saves the given stream to the given path, computes the resulting
        /// file's SHA1 hash and returns it.
        /// </summary>
        /// <param name="stream">The stream to save and compute the hash of.</param>
        /// <param name="path">The path to save the stream's contents to.</param>
        /// <returns>The SHA1 hash of the given stream's file.</returns>
        public static string SaveAndHash(this Stream stream, string path)
        {
            using (FileStream fileStream = File.Create(path))
            {
                byte[] buffer = new byte[4096];
                int count = 0;

                while (0 < (count = stream.Read(buffer, 0, buffer.Length)))
                {
                    fileStream.Write(buffer, 0, count);
                }

                return fileStream.Hash();
            }
        }
    }
}
