//-----------------------------------------------------------------------
// <copyright file="Primitives.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides extensions and helpers for primitive and built-in types.
    /// </summary>
    public static class Primitives
    {
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
        /// Splits the given string on the given character, removing any empty 
        /// results and trimming whitespace around the rest of the results.
        /// </summary>
        /// <param name="value">The string value to split.</param>
        /// <param name="separator">The character to split the string on.</param>
        /// <returns>The split string.</returns>
        public static string[] SplitAndTrim(this string value, char separator)
        {
            return (from s in value.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                    select s.Trim()).ToArray();
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
    }
}
