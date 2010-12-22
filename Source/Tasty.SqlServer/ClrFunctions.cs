//-----------------------------------------------------------------------
// <copyright file="ClrFunctions.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.SqlServer
{
    using System;
    using System.Collections;
    using System.Data.SqlTypes;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.RegularExpressions;
    using Microsoft.SqlServer.Server;

    /// <summary>
    /// A collection of helper functions that can be imported to SQL Server.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "Required class format for SQL Server CLR integration.")]
    public partial class ClrFunctions
    {
        /// <summary>
        /// Gets a value indicating whether the given input string matches the given regular expression.
        /// </summary>
        /// <param name="input">The input to check against the regular expression.</param>
        /// <param name="pattern">The regular expression patter to check.</param>
        /// <param name="ignoreCase">A value indicating whether to ignore case.</param>
        /// <param name="multiline">A value indicating whether to treat the input as multiline.</param>
        /// <returns>True if the input string matches the regular expression, false otherwise.</returns>
        [SqlFunction(IsDeterministic = true, IsPrecise = true)]
        public static SqlBoolean RegexIsMatch(string input, string pattern, bool ignoreCase, bool multiline)
        {
            input = input ?? String.Empty;
            pattern = pattern ?? String.Empty;

            return Regex.IsMatch(input, pattern, CreateRegexOptions(ignoreCase, multiline));
        }

        /// <summary>
        /// Replaces matches of the given regulare expression string on the given input string with the given replacement string.
        /// </summary>
        /// <param name="input">The input string to replace matches on.</param>
        /// <param name="pattern">The regular expression pattern identifying the replacements to make.</param>
        /// <param name="replacement">The replacement string to use.</param>
        /// <param name="ignoreCase">A value indicating whether to ignore case.</param>
        /// <param name="multiline">A value indicating whether to treat the input as multiline.</param>
        /// <returns>The result of the replacement operation.</returns>
        public static SqlString RegexReplace(string input, string pattern, string replacement, bool ignoreCase, bool multiline)
        {
            input = input ?? String.Empty;
            pattern = pattern ?? String.Empty;

            return Regex.Replace(input, pattern, replacement, CreateRegexOptions(ignoreCase, multiline));
        }

        /// <summary>
        /// Splits the given input string using the given regular expression.
        /// </summary>
        /// <param name="input">The input string to split.</param>
        /// <param name="pattern">The regular expression pattern to split the string on.</param>
        /// <param name="ignoreCase">A value indicating whether to ignore case.</param>
        /// <param name="multiline">A value indicating whether to treat the input as multiline.</param>
        /// <returns>The input string split using the given regular expression.</returns>
        [SqlFunction(IsDeterministic = true, IsPrecise = true, FillRowMethodName = "FillRegexSplitRow")]
        public static IEnumerable RegexSplit(string input, string pattern, bool ignoreCase, bool multiline)
        {
            input = input ?? String.Empty;
            pattern = pattern ?? String.Empty;

            return Regex.Split(input, pattern, CreateRegexOptions(ignoreCase, multiline));
        }

        /// <summary>
        /// Gets a value indicating whether any of the values in a string set are equal to any
        /// of the values in a second string set. The string sets are tokenized by the given separator,
        /// which will default to the newline (\n) character if null or empty.
        /// </summary>
        /// <param name="referenceSet">The reference string set to check.</param>
        /// <param name="askingSet">The asking string set to check.</param>
        /// <param name="separator">The separator to use when splitting the strings into sets.</param>
        /// <returns>True if any of the values are equal, false otherwise.</returns>
        [SqlFunction(IsDeterministic = true, IsPrecise = true)]
        public static SqlBoolean StringSetContainsAny(string referenceSet, string askingSet, string separator)
        {
            referenceSet = (referenceSet ?? String.Empty).Trim();
            askingSet = (askingSet ?? String.Empty).Trim();
            separator = separator ?? String.Empty;

            if (String.IsNullOrEmpty(separator))
            {
                separator = "\n";
            }

            string[] separatros = new string[] { separator };
            bool contains = String.IsNullOrEmpty(referenceSet) && String.IsNullOrEmpty(askingSet);

            if (!contains)
            {
                string[] reference = referenceSet.Split(separatros, StringSplitOptions.RemoveEmptyEntries);
                string[] asking = askingSet.Split(separatros, StringSplitOptions.RemoveEmptyEntries);

                foreach (string r in reference)
                {
                    string rtr = r.Trim();

                    if (!String.IsNullOrEmpty(rtr))
                    {
                        foreach (string a in asking)
                        {
                            string atr = a.Trim();

                            if (!String.IsNullOrEmpty(atr) && rtr.Equals(atr, StringComparison.OrdinalIgnoreCase))
                            {
                                contains = true;
                                break;
                            }
                        }
                    }

                    if (contains)
                    {
                        break;
                    }
                }
            }

            return contains;
        }

        /// <summary>
        /// Creates a <see cref="RegexOptions"/> enumeration using the given indicators.
        /// </summary>
        /// <param name="ignoreCase">A value indicating whether to include <see cref="RegexOptions.IgnoreCase"/>.</param>
        /// <param name="multiline">A value indicating whether to include <see cref="RegexOptions.Multiline"/>.</param>
        /// <returns>The created <see cref="RegexOptions"/>.</returns>
        private static RegexOptions CreateRegexOptions(bool ignoreCase, bool multiline)
        {
            RegexOptions options = RegexOptions.None;

            if (ignoreCase)
            {
                options = options | RegexOptions.IgnoreCase;
            }

            if (multiline)
            {
                options = options | RegexOptions.Multiline;
            }

            return options;
        }

        /// <summary>
        /// Fills a row generated by <see cref="RegexSplit"/>.
        /// </summary>
        /// <param name="obj">The object containing values to be used when filling the row.</param>
        /// <param name="part">The first column value.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This method is called internally by the runtime.")]
        private static void FillRegexSplitRow(object obj, out string part)
        {
            part = (string)((object[])obj)[0];
        }
    }
}
