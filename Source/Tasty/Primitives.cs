//-----------------------------------------------------------------------
// <copyright file="Primitives.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
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
    using System.Reflection;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides extensions and helpers for primitive and built-in types.
    /// </summary>
    public static class Primitives
    {
        private const string TypeExpression = @"^([^,]+)(,([^,]+).*)?";

        /// <summary>
        /// Copies any same-named property values from the source object to the destination object.
        /// Each destination property must be of a type that is assignable from the type
        /// of the corresponding source property.
        /// </summary>
        /// <param name="source">The source object to copy properties from.</param>
        /// <param name="destination">The destination object to copy properties to.</param>
        public static void CopyProperties(this object source, object destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "source must have a value.");
            }

            if (destination == null)
            {
                throw new ArgumentNullException("destination", "destination must have a value.");
            }

            var props = from s in source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        join d in destination.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public) on s.Name equals d.Name
                        select new
                        {
                            SourceProp = s,
                            DestProp = d
                        };

            foreach (var prop in props)
            {
                if (prop.DestProp.CanWrite && prop.SourceProp.CanRead)
                {
                    object value = prop.SourceProp.GetValue(source, null);

                    if (prop.DestProp.PropertyType.IsAssignableFrom(prop.SourceProp.PropertyType))
                    {
                        prop.DestProp.SetValue(destination, value, null);
                    }
                }
            }
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
        /// De-serializes a string of JSON into an object of the given type.
        /// </summary>
        /// <param name="type">The type of object to de-serialize the given JSON into.</param>
        /// <param name="value">A string of JSON to de-serialize.</param>
        /// <returns>The de-serialized object.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public static object FromJson(Type type, string value)
        {
            return FromJson(type, value, new List<Type>());
        }

        /// <summary>
        /// De-serializes a string of JSON into an object of the given type.
        /// </summary>
        /// <param name="type">The type of object to de-serialize the given JSON into.</param>
        /// <param name="value">A string of JSON to de-serialize.</param>
        /// <param name="knownTypes">A collection of known types the serializer may encounter in the object graph.</param>
        /// <returns>The de-serialized object.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public static object FromJson(Type type, string value, IEnumerable<Type> knownTypes)
        {
            return FromJson(type, value, Encoding.Default, knownTypes);
        }

        /// <summary>
        /// De-serializes a string of JSON into an object of the given type.
        /// </summary>
        /// <param name="type">The type of object to de-serialize the given JSON into.</param>
        /// <param name="value">A string of JSON to de-serialize.</param>
        /// <param name="encoding">The encoding the JSON string is in.</param>
        /// <param name="knownTypes">A collection of known types the serializer may encounter in the object graph.</param>
        /// <returns>The de-serialized object.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public static object FromJson(Type type, string value, Encoding encoding, IEnumerable<Type> knownTypes)
        {
            if (!String.IsNullOrEmpty(value))
            {
                using (Stream stream = new MemoryStream())
                {
                    using (StreamWriter writer = new StreamWriter(stream, encoding))
                    {
                        writer.Write(value);
                        writer.Flush();
                        stream.Position = 0;

                        return new DataContractJsonSerializer(type, knownTypes).ReadObject(stream);
                    }
                }
            }
            else
            {
                return Activator.CreateInstance(type);
            }
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
        /// Strips all HTML tags from the given string value.
        /// </summary>
        /// <param name="value">The string value to strip HTML tags from.</param>
        /// <returns>The string with HTML stripped.</returns>
        public static string StripHtml(this string value)
        {
            return Regex.Replace(value, @"<\/?[^>]+>", String.Empty);
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
        /// Converts a length of bytes to a friendly file size string.
        /// </summary>
        /// <param name="bytes">The bytes length to convert.</param>
        /// <returns>A friendly file size string.</returns>
        public static string ToFileSize(this long bytes)
        {
            const decimal KB = 1024;
            const decimal MB = KB * 1024;
            const decimal GB = MB * 1024;

            decimal size = Convert.ToDecimal(bytes);
            string suffix = " B";
            string format = "N0";

            if (bytes > GB)
            {
                size /= GB;
                suffix = " GB";
                format = "N2";
            }
            else if (bytes > MB)
            {
                size /= MB;
                suffix = " MB";
                format = "N1";
            }
            else if (bytes > KB)
            {
                size /= KB;
                suffix = " KB";
            }

            return size.ToString(format, CultureInfo.InvariantCulture) + suffix;
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
        /// Returns a string representation of the given DateTime object
        /// that conforms to ISO 8601 (in UTC).
        /// </summary>
        /// <param name="dateTime">The DateTime object to convert.</param>
        /// <returns>A string representing the date in ISO 8601 format.</returns>
        public static string ToIso8601UtcString(this DateTime dateTime)
        {
            dateTime = dateTime.ToUniversalTime();
            return String.Format(CultureInfo.InvariantCulture, "{0:s}.{0:fff}Z", dateTime);
        }

        /// <summary>
        /// Returns a string representation of the given DateTime object
        /// that conforms to ISO 8601 (in UTC), replacing colons and periods
        /// with dashes for use in filenames.
        /// </summary>
        /// <param name="dateTime">The DateTime object to convert.</param>
        /// <returns>A string representing the date in ISO 8601 format with unsaf path characters removed.</returns>
        public static string ToIso8601UtcPathSafeString(this DateTime dateTime)
        {
            return Regex.Replace(dateTime.ToIso8601UtcString(), @"[\.:]", "-");
        }

        /// <summary>
        /// Serializes the given object to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of the object being serialized.</typeparam>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A string of JSON.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public static string ToJson<T>(this T value)
        {
            return ToJson(value, new List<Type>());
        }

        /// <summary>
        /// Serializes the given object to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of the object being serialized.</typeparam>
        /// <param name="value">The object to serialize.</param>
        /// <param name="knownTypes">A collection of known types to feed the serializer that may be found in the object graph.</param>
        /// <returns>A string of JSON.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public static string ToJson<T>(this T value, IEnumerable<Type> knownTypes)
        {
            return ToJson(typeof(T), value, knownTypes);
        }

        /// <summary>
        /// Serializes the given object to a JSON string.
        /// </summary>
        /// <param name="type">The type of the object being serialized.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="knownTypes">A collection of known types to feed the serializer that may be found in the object graph.</param>
        /// <returns>A string of JSON.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public static string ToJson(Type type, object value, IEnumerable<Type> knownTypes)
        {
            if (value != null)
            {
                using (Stream stream = new MemoryStream())
                {
                    new DataContractJsonSerializer(type, knownTypes).WriteObject(stream, value);
                    stream.Position = 0;

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the spreadhseet column name (i.e., A or AB or EF) for the given column number.
        /// </summary>
        /// <param name="columnNumber">The column number (the first column is 1, not 0).</param>
        /// <returns>The spreadsheet column name.</returns>
        public static string ToSpreadsheetColumnName(this int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        /// <summary>
        /// Gets the type name from the give type name string without any assembly information.
        /// </summary>
        /// <param name="typeName">The type name string to pull the bare type name from.</param>
        /// <returns>A type name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when typeName is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when typeName doesn't represent a valid type string.</exception>
        public static string TypeNameWithoutAssembly(this string typeName)
        {
            return MatchTypeName(typeName).Groups[1].Value;
        }

        /// <summary>
        /// Gets the regular expression match for the given type name.
        /// </summary>
        /// <param name="typeName">The type name to get the regular expression match for.</param>
        /// <returns>A regular expression match.</returns>
        private static Match MatchTypeName(string typeName)
        {
            if (String.IsNullOrEmpty(typeName))
            {
                throw new ArgumentNullException("typeName", "typeName must have a value.");
            }

            Match match = Regex.Match(typeName, TypeExpression);

            if (match.Success)
            {
                return match;
            }
            else
            {
                throw new ArgumentException("typeName does not appear to be a valid type name", "typeName");
            }
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
