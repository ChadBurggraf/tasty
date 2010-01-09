using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Tasty.Build
{
    /// <summary>
    /// <see cref="Tasty.Build"/> extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Splits a database connection string into a collection of key/value pairs.
        /// The resulting keys are all normalized to lower-case.
        /// </summary>
        /// <param name="connectionString">The connection string to split.</param>
        /// <returns>The connection string as a collection of key/value pairs.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "It seems more intuitive to access the result collection via lower-case.")]
        public static NameValueCollection SplitConnectionString(this string connectionString)
        {
            NameValueCollection coll = new NameValueCollection();

            if (!String.IsNullOrEmpty(connectionString))
            {
                foreach (string pair in connectionString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] keyValue = pair.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);

                    if (keyValue.Length > 0)
                    {
                        string key = keyValue[0].ToLowerInvariant();
                        string value = null;

                        if (keyValue.Length > 1)
                        {
                            value = keyValue[1];
                        }

                        coll[key] = value;
                    }
                }
            }

            return coll;
        }

        /// <summary>
        /// Splits a string of SQL commands on "GO" to enable issuing them
        /// individually using ADO.
        /// </summary>
        /// <param name="sql">The SQL command set to split.</param>
        /// <returns>A collection of SQL commands.</returns>
        public static IList<string> SplitSqlCommands(this string sql)
        {
            // Split the resource into individual commands on "GO".
            var parts = from c in Regex.Split(sql, @"[;\s]+GO([;\s]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline)
                        select c.Trim();

            return parts.Where(c => !String.IsNullOrEmpty(c)).ToArray();
        }

        /// <summary>
        /// Gets the given collection as a database connection string (key/values paired with '=' and separated by ';').
        /// </summary>
        /// <param name="collection">The connection string collection to convert.</param>
        /// <returns>A connection string.</returns>
        public static string ToConnectionString(this NameValueCollection collection)
        {
            const string ESCAPE_EXP = "[;=]+";
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < collection.AllKeys.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(";");
                }

                string key = collection.AllKeys[i];
                string value = collection[key];

                sb.AppendFormat("{0}={1}", Regex.Replace(key, ESCAPE_EXP, String.Empty), Regex.Replace(value, ESCAPE_EXP, String.Empty));
            }

            return sb.ToString();
        }
    }
}
