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
        /// Safely raises an event on an object by first checking if the handler is null.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="handler">The event delegate.</param>
        /// <param name="e">The event arguments.</param>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "Not appropriate here.")]
        public static void RaiseEvent(this object sender, EventHandler handler, EventArgs e)
        {
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        /// <summary>
        /// Safely raises an event on an object by first checking if the handler is null.
        /// </summary>
        /// <typeparam name="T">The type of the event arguments for the generic event being raised.</typeparam>
        /// <param name="sender">The event sender.</param>
        /// <param name="handler">The event delegate.</param>
        /// <param name="e">The event arguments.</param>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "Not appropriate here.")]
        public static void RaiseEvent<T>(this object sender, EventHandler<T> handler, T e) where T : EventArgs
        {
            if (handler != null)
            {
                handler(sender, e);
            }
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
    }
}
