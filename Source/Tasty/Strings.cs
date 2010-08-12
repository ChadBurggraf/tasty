//-----------------------------------------------------------------------
// <copyright file="Strings.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides extensions and helpers for strings.
    /// </summary>
    public static class Strings
    {
        /// <summary>
        /// Escapes the given string for use in a filesystem or URL path.
        /// </summary>
        /// <param name="pathPart">The path part to escape</param>
        /// <returns>The escaped path part.</returns>
        public static string EscapeForPath(this string pathPart)
        {
            return EscapeForPath(pathPart, Char.MinValue);
        }

        /// <summary>
        /// Escapes the given string for use in a filesystem or URL path.
        /// If pathSeparator is not <see cref="Char.MinValue"/>, will 
        /// normalize all path separator characters to the given value and leave the
        /// path hierarchy intact.
        /// </summary>
        /// <param name="pathPart">The path or path part to escape.</param>
        /// <param name="pathSeparator">The path separator, or <see cref="Char.MinValue"/> to escape as a single path part.</param>
        /// <returns>The escaped path.</returns>
        public static string EscapeForPath(this string pathPart, char pathSeparator)
        {
            const string BaseExp = @"[^0-9a-z]";

            if (!String.IsNullOrEmpty(pathPart))
            {
                if (pathSeparator != Char.MinValue)
                {
                    string[] pathParts = pathPart
                        .Split(new char[] { '/', '\\' })
                        .Select(s => Regex.Replace(s, BaseExp, " ", RegexOptions.IgnoreCase))
                        .ToArray();

                    pathPart = String.Join(pathSeparator.ToString(CultureInfo.InvariantCulture), pathParts);
                }
                else
                {
                    pathPart = Regex.Replace(pathPart, BaseExp, " ", RegexOptions.IgnoreCase);
                }

                pathPart = Regex.Replace(pathPart, @"\s+", "-");
            }

            return pathPart;
        }

        /// <summary>
        /// Converts the given hex string to an array of bytes.
        /// </summary>
        /// <param name="hex">The hex string to convert.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] FromHex(this string hex)
        {
            byte[] buffer = new byte[hex.Length / 2];

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return buffer;
        }

        /// <summary>
        /// Computes the SHA1 hash of the string.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hashed text.</returns>
        public static string Hash(this string value)
        {
            return Hash(value, Encoding.UTF8);
        }

        /// <summary>
        /// Computes the SHA1 hash of the string.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns>The hashed text.</returns>
        public static string Hash(this string value, Encoding encoding)
        {
            return Hash(value, encoding, ShaLevel.One);
        }

        /// <summary>
        /// Computes an SHA hash of the string.
        /// </summary>
        /// <param name="value">The string to hash.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <param name="level">The SHA level to use.</param>
        /// <returns>The hashed value.</returns>
        public static string Hash(this string value, Encoding encoding, ShaLevel level)
        {
            HashAlgorithm crypto;

            switch (level)
            {
                case (ShaLevel.TwoFiftySix):
                    crypto = new SHA256CryptoServiceProvider();
                    break;
                case (ShaLevel.FiveTwelve):
                    crypto = new SHA512CryptoServiceProvider();
                    break;
                default:
                    crypto = new SHA1CryptoServiceProvider();
                    break;
            }

            byte[] buffer = encoding.GetBytes(value);
            return BitConverter.ToString(crypto.ComputeHash(buffer)).Replace("-", String.Empty);
        }

        /// <summary>
        /// Splits the given string on the given character, removing any empty 
        /// results and trimming whitespace around the rest of the results.
        /// </summary>
        /// <param name="value">The string value to split.</param>
        /// <param name="separator">The character to split the string on.</param>
        /// <returns>The split string.</returns>
        public static string[] SplitAndTrim(this string value, char separator)
        {
            return SplitAndTrim(value, new char[] { separator });
        }

        /// <summary>
        /// Splits the given string on the given separator, removing any empty 
        /// results and trimming whitespace around the rest of the results.
        /// </summary>
        /// <param name="value">The string value to split.</param>
        /// <param name="separator">The string to split the string on.</param>
        /// <returns>The split string.</returns>
        public static string[] SplitAndTrim(this string value, string separator)
        {
            if (String.IsNullOrEmpty(separator))
            {
                throw new ArgumentNullException("separator", "separator must have a value.");
            }

            return SplitAndTrim(value, separator.ToCharArray());
        }

        /// <summary>
        /// Splits a semi-colon separated connection string into a dictionary of key-value pairs.
        /// </summary>
        /// <param name="connectionString">The connection string to split.</param>
        /// <returns>A dictionary of key-value pairs.</returns>
        public static Dictionary<string, string> SplitConnectionString(string connectionString)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            char[] semi = new char[] { ';' };
            char[] eq = new char[] { '=' };

            if (!String.IsNullOrEmpty(connectionString))
            {
                string[] pairs = connectionString.Split(semi, StringSplitOptions.RemoveEmptyEntries);

                foreach (string pair in pairs)
                {
                    string[] keyValue = pair.Split(eq, StringSplitOptions.RemoveEmptyEntries);
                    string key = keyValue[0].Trim();
                    string value = keyValue.Length > 1 ? keyValue[1].Trim() : String.Empty;

                    if (!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(value))
                    {
                        dict[key] = value;
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// Combines the string dictionary with semi-colon (;) separators to form a connection string.
        /// </summary>
        /// <param name="dict">The string dictionary to combine.</param>
        /// <returns>The combined connection string.</returns>
        public static string ToConnectionString(this IDictionary<string, string> dict)
        {
            StringBuilder sb = new StringBuilder();

            if (dict != null)
            {
                foreach (KeyValuePair<string, string> pair in dict)
                {
                    string key = pair.Key.Trim().Replace(";", String.Empty);
                    string value = pair.Value.Trim().Replace(";", String.Empty);

                    if (!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(value))
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "{0}={1};", key, value);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the first part of an email address for use as a name.
        /// </summary>
        /// <param name="email">The email address to get the name part of.</param>
        /// <returns>An email name.</returns>
        public static string ToEmailName(this string email)
        {
            return Regex.Replace(email ?? String.Empty, @"@.*$", String.Empty);
        }

        /// <summary>
        /// Converts the given byte array to a hex string.
        /// </summary>
        /// <param name="buffer">The byte array to convert.</param>
        /// <returns>A hex string.</returns>
        public static string ToHex(this byte[] buffer)
        {
            StringBuilder sb = new StringBuilder(buffer.Length * 2);

            foreach (byte b in buffer)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets a URL-friendly index name from the given name.
        /// </summary>
        /// <param name="name">The name to escape.</param>
        /// <returns>The escaped name.</returns>
        public static string ToIndexName(this string name)
        {
            return Regex.Replace(Regex.Replace(name, @"[^0-9a-z]", " ", RegexOptions.IgnoreCase).Trim(), @"\s+", "-");
        }

        /// <summary>
        /// Splits the given string on the given separator characters, removing any empty 
        /// results and trimming whitespace around the rest of the results.
        /// </summary>
        /// <param name="value">The string value to split.</param>
        /// <param name="separator">The separator characters to split the string on.</param>
        /// <returns>The split string.</returns>
        private static string[] SplitAndTrim(string value, char[] separator)
        {
            if (separator == null || separator.Length == 0)
            {
                throw new ArgumentException("separator must have a value and be at least 1 element long.", "separator");
            }

            return (from s in (value ?? String.Empty).Split(separator, StringSplitOptions.RemoveEmptyEntries)
                    select s.Trim()).ToArray();
        }
    }
}
