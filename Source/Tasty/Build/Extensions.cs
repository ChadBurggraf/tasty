//-----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Build
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// <see cref="Tasty.Build"/> extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the regular expression pattern used for matching AssemblyFileVersion attributes in AssemblyInfo files.
        /// </summary>
        internal const string AssemblyFileVersionPattern = @"(AssemblyFileVersion\s*?\()([^\)]+)(\))";

        /// <summary>
        /// Gets the regular expression pattern used for matching AssemblyVersion attributes in AssemblyInfo files.
        /// </summary>
        internal const string AssemblyVersionPattern = @"(AssemblyVersion\s*?\()([^\)]+)(\))";

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
            List<string> commands = new List<string>();
            List<char> buffer = new List<char>();
            bool inString = false;

            Func<int> goIndex = () =>
            {
                char prev = '\0', curr;

                for (int i = buffer.Count - 1; i >= 0; --i)
                {
                    curr = Char.ToUpperInvariant(buffer[i]);

                    if (!Char.IsWhiteSpace(curr))
                    {
                        if (curr == 'G')
                        {
                            if (prev == 'O')
                            {
                                return i;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else if (curr != 'O')
                        {
                            break;
                        }
                    }

                    prev = curr;
                }

                return -1;
            };

            foreach (char c in sql) 
            {
                if (c == '\'')
                {
                    inString = !inString;
                }
                else if (c == '\n' && !inString)
                {
                    int gi = goIndex();

                    if (gi >= 0)
                    {
                        string command = new string(buffer.ToArray()).Substring(0, gi).Trim();

                        if (!String.IsNullOrEmpty(command))
                        {
                            commands.Add(command);
                        }

                        buffer.Clear();
                        continue;
                    }
                }

                buffer.Add(c);
            }

            if (buffer.Count > 0)
            {
                string command = new string(buffer.ToArray());
                int gi = goIndex();

                if (gi >= 0)
                {
                    command = command.Substring(0, gi);
                }

                command = command.Trim();

                if (!String.IsNullOrEmpty(command))
                {
                    commands.Add(command);
                }
            }

            return commands.ToArray();
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
