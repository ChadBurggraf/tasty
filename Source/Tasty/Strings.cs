﻿//-----------------------------------------------------------------------
// <copyright file="Strings.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    /// <summary>
    /// Provides extensions and helpers for strings.
    /// </summary>
    public static class Strings
    {
        /// <summary>
        /// A regular expression that can be used to validate email addresses.
        /// </summary>
        public const string EmailExpression = @"^[a-z0-9._%'+-]+@[a-z0-9.-]+\.[a-z]{2,4}$";

        /// <summary>
        /// A rough regular expression for validating URLs.
        /// </summary>
        public const string UrlExpression = @"^((https?://)|(~/))" // http or asp ~/
            + @"?(([0-9a-z_!~*'().&=+$%-]+: )?[0-9a-z_!~*'().&=+$%-]+@)?" // user@
            + @"localhost|"
            + @"(([0-9]{1,3}\.){3}[0-9]{1,3}" // IP- 199.194.52.184
            + @"|" // allows either IP or domain
            + @"([0-9a-z_!~*'()-]+\.)*" // tertiary domain(s)- www.
            + @"([0-9a-z][0-9a-z-]{0,61})?[0-9a-z]\." // second level domain
            + @"[a-z]{2,6})" // first level domain- .com or .museum
            + @"(:[0-9]{1,4})?" // port number- :80
            + @"((/?)|" // a slash isn't required if there is no file name
            + @"(/[0-9a-z_!~*'().;?:@&=+$,%#-]+)+/?)$";

        /// <summary>
        /// Alias of <see cref="String.IndexOf(String)"/> to get a value indicating whether the string contains
        /// the given string value using the specified comparison type.
        /// </summary>
        /// <param name="original">The original string to compare.</param>
        /// <param name="value">The string value to find in the original string.</param>
        /// <param name="comparisonType">The comparison type to use.</param>
        /// <returns>True if the value is found in the original string, false otherwise.</returns>
        public static bool Contains(this string original, string value, StringComparison comparisonType)
        {
            return !String.IsNullOrEmpty(value) && original.IndexOf(value, comparisonType) >= 0;
        }

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
        /// Gets the decoded value of the base-64 string.
        /// </summary>
        /// <param name="value">A base-64 encoded string.</param>
        /// <returns>The decoded string value.</returns>
        public static string FromBase64(this string value)
        {
            Decoder decoder = Encoding.UTF8.GetDecoder();

            byte[] buffer = Convert.FromBase64String(value);
            char[] chars = new char[decoder.GetCharCount(buffer, 0, buffer.Length)];
            decoder.GetChars(buffer, 0, buffer.Length, chars, 0);

            return new string(chars);
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
        /// Converts the lower_case_underscore string into a PascalCase or camelCalse string.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>The converted string.</returns>
        public static string FromLowercaseUnderscore(this string value)
        {
            return FromLowercaseUnderscore(value, false);
        }

        /// <summary>
        /// Converts the lower_case_underscore string into a PascalCase or camelCalse string.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="camel">A value indicating whether to convert to camelCalse. If false, will convert to PascalCase.</param>
        /// <returns>The converted string.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Explicitly converting from an all lowercase string.")]
        public static string FromLowercaseUnderscore(this string value, bool camel)
        {
            return value.FromLowercaseWithSeparator('_', camel);
        }

        /// <summary>
        /// Converts the lower case string (with word boundaries separated by the given separator character)
        /// into a PascalCase or camelCalse string.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="separator">The character used to denote word boundaries.</param>
        /// <param name="camel">A value indicating whether to convert to camelCalse. If false, will convert to PascalCase.</param>
        /// <returns>The converted string.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Explicitly converting from an all lowercase string.")]
        public static string FromLowercaseWithSeparator(this string value, char separator, bool camel)
        {
            value = (value ?? String.Empty).ToLowerInvariant().Trim();

            if (String.IsNullOrEmpty(value))
            {
                return value;
            }

            StringBuilder sb = new StringBuilder();
            int i = 0;
            int wordLetterNumber = 0;

            while (i < value.Length)
            {
                if (Char.IsLetterOrDigit(value, i))
                {
                    wordLetterNumber++;
                }
                else
                {
                    wordLetterNumber = 0;
                }

                if (wordLetterNumber == 1)
                {
                    if (camel && i == 0)
                    {
                        sb.Append(value[i]);
                    }
                    else
                    {
                        sb.Append(value[i].ToString().ToUpperInvariant());
                    }
                }
                else if (value[i] != separator)
                {
                    sb.Append(value[i]);
                }

                i++;
            }

            return sb.ToString();
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
                case ShaLevel.TwoFiftySix:
                    crypto = new SHA256CryptoServiceProvider();
                    break;
                case ShaLevel.FiveTwelve:
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
        /// Encodes the value for use in an HTML/XML attribute.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <returns>The encoded value.</returns>
        public static string HtmlAttributeEncode(this string value)
        {
            return HttpUtility.HtmlAttributeEncode(value ?? String.Empty);
        }

        /// <summary>
        /// Decodes the given HTML/XML-encoded value.
        /// </summary>
        /// <param name="value">The value to decode.</param>
        /// <returns>The decoded value.</returns>
        public static string HtmlDecode(this string value)
        {
            return HttpUtility.HtmlDecode(value ?? String.Empty);
        }

        /// <summary>
        /// Encodes the value for use in an HTML/XML element.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <returns>The encoded value.</returns>
        public static string HtmlEncode(this string value)
        {
            return HttpUtility.HtmlEncode(value ?? String.Empty);
        }

        /// <summary>
        /// Gets a value indicating whether the string contains a valid email address.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the string is a valid email address, false otherwise.</returns>
        public static bool IsValidEmail(this string value)
        {
            return Regex.IsMatch(value, EmailExpression, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Gets a value indicating whether the string contains a valid URL.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the string is a valid URL, false otherwise.</returns>
        public static bool IsValidUrl(this string value)
        {
            return Regex.IsMatch(value, UrlExpression, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Calculates the MD5 hash of a string.
        /// </summary>
        /// <param name="value">The string value to get the MD5 hash of.</param>
        /// <returns>An MD5 hash.</returns>
        public static string MD5Hash(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value", "value must contain a value.");
            }

            byte[] input = Encoding.UTF8.GetBytes(value);
            byte[] hash = MD5.Create().ComputeHash(input);
            StringBuilder hex = new StringBuilder();

            foreach (byte b in hash)
            {
                hex.Append(b.ToString("X2"));
            }

            return hex.ToString();
        }

        /// <summary>
        /// Roots the path with the current value of <see cref="Environment.CurrentDirectory"/>, if it is not null or empty and is not already rooted.
        /// </summary>
        /// <param name="path">The path to root.</param>
        /// <returns>The rooted path, or null or empty if the original path was null or empty.</returns>
        public static string RootPath(this string path)
        {
            return RootPath(path, Environment.CurrentDirectory);
        }

        /// <summary>
        /// Roots the path with the given path root, if it is not null or empty and is not already rooted.
        /// </summary>
        /// <param name="path">The path to root.</param>
        /// <param name="pathRoot">The rooth path to apply.</param>
        /// <returns>The rooted path, or null or empty if the original path was null or empty.</returns>
        public static string RootPath(this string path, string pathRoot)
        {
            if (!String.IsNullOrEmpty(path))
            {
                if (!Path.IsPathRooted(path))
                {
                    if (String.IsNullOrEmpty(pathRoot))
                    {
                        throw new ArgumentNullException("pathRoot", "pathRoot must contain a value.");
                    }

                    if (!Path.IsPathRooted(pathRoot))
                    {
                        throw new ArgumentException("pathRoot must be a rooted path.", "pathRoot");
                    }

                    path = Path.Combine(pathRoot, path);
                }
            }

            return path;
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
            return SplitAndTrim(value, separator, false);
        }

        /// <summary>
        /// Splits the given string on the given character, removing any empty 
        /// results and trimming whitespace around the rest of the results.
        /// </summary>
        /// <param name="value">The string value to split.</param>
        /// <param name="separator">The character to split the string on.</param>
        /// <param name="removeEmptyEntries">A value indicating whether to remove empty entries from the results.</param>
        /// <returns>The split string.</returns>
        public static string[] SplitAndTrim(this string value, char separator, bool removeEmptyEntries)
        {
            return SplitAndTrim(value, new char[] { separator }, removeEmptyEntries);
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
            return SplitAndTrim(value, separator, false);
        }

        /// <summary>
        /// Splits the given string on the given separator, removing any empty 
        /// results and trimming whitespace around the rest of the results.
        /// </summary>
        /// <param name="value">The string value to split.</param>
        /// <param name="separator">The string to split the string on.</param>
        /// <param name="removeEmptyEntries">A value indicating whether to remove empty entries from the results.</param>
        /// <returns>The split string.</returns>
        public static string[] SplitAndTrim(this string value, string separator, bool removeEmptyEntries)
        {
            if (String.IsNullOrEmpty(separator))
            {
                throw new ArgumentNullException("separator", "separator must have a value.");
            }

            return SplitAndTrim(value, separator.ToCharArray(), removeEmptyEntries);
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
        /// Strips all HTML tags from the given string value.
        /// </summary>
        /// <param name="value">The string value to strip HTML tags from.</param>
        /// <returns>The string with HTML stripped.</returns>
        public static string StripHtml(this string value)
        {
            return Regex.Replace(value, @"<\/?[^>]+>", String.Empty);
        }

        /// <summary>
        /// Base-64 encodes the string value.
        /// </summary>
        /// <param name="value">The string value to encode.</param>
        /// <returns>The base-64 encoded value.</returns>
        public static string ToBase64(this string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// Combines the string dictionary with semi-colon (;) separators to form a connection string.
        /// </summary>
        /// <param name="dictionary">The string dictionary to combine.</param>
        /// <returns>The combined connection string.</returns>
        public static string ToConnectionString(this IDictionary<string, string> dictionary)
        {
            StringBuilder sb = new StringBuilder();

            if (dictionary != null)
            {
                foreach (KeyValuePair<string, string> pair in dictionary)
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
        /// Converts the camelCase or PascalCase string to a lower_case_underscore string.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>The converted string.</returns>
        public static string ToLowercaseUnderscore(this string value)
        {
            return value.ToLowercaseWithSeparator('_');
        }

        /// <summary>
        /// Converts the camelCase or PascalCase string to a lower-case string with a separator 
        /// (e.g., camel_case or pascal-case, depending on the separator).
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="separator">The separator to use.</param>
        /// <returns>The converted string.</returns>
        public static string ToLowercaseWithSeparator(this string value, char separator)
        {
            value = (value ?? String.Empty).Trim();

            if (String.IsNullOrEmpty(value))
            {
                return value;
            }

            StringBuilder sb = new StringBuilder();
            int i = 0;
            int wordLetterNumber = 0;
            bool prevUpper = false;

            while (i < value.Length)
            {
                if (Char.IsLetterOrDigit(value, i))
                {
                    wordLetterNumber++;
                }
                else
                {
                    wordLetterNumber = 0;
                }

                if (Char.IsUpper(value, i))
                {
                    if (wordLetterNumber > 1 && !prevUpper)
                    {
                        sb.Append(separator);
                    }

                    sb.Append(Char.ToLowerInvariant(value[i]));
                    prevUpper = true;
                }
                else
                {
                    sb.Append(value[i]);
                    prevUpper = false;
                }

                i++;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Replaces the file extension on the path with the given extension. If no file extension
        /// exists on the given path, then appends the given extension.
        /// </summary>
        /// <param name="path">The path to replace the extension of.</param>
        /// <param name="extension">The file extension to set, without a "."</param>
        /// <returns>The updated path.</returns>
        public static string WithExtension(this string path, string extension)
        {
            if (!String.IsNullOrEmpty(path))
            {
                string ext = Path.GetExtension(path);
                path = String.Concat(path.Substring(0, path.Length - ext.Length), ".", extension);
            }

            return path;
        }

        /// <summary>
        /// Splits the given string on the given separator characters, removing any empty 
        /// results and trimming whitespace around the rest of the results.
        /// </summary>
        /// <param name="value">The string value to split.</param>
        /// <param name="separator">The separator characters to split the string on.</param>
        /// <param name="removeEmptyEntries">A value indicating whether to remove empty entries from the result.</param>
        /// <returns>The split string.</returns>
        private static string[] SplitAndTrim(string value, char[] separator, bool removeEmptyEntries)
        {
            if (separator == null || separator.Length == 0)
            {
                throw new ArgumentException("separator must have a value and be at least 1 element long.", "separator");
            }

            return (from s in (value ?? String.Empty).Split(separator, StringSplitOptions.RemoveEmptyEntries)
                    let result = s.Trim()
                    where !removeEmptyEntries || !String.IsNullOrEmpty(result)
                    select result).ToArray();
        }
    }
}
