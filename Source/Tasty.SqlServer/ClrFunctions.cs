//-----------------------------------------------------------------------
// <copyright file="ClrFunctions.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.SqlServer
{
    using System;
    using System.Data.SqlTypes;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.SqlServer.Server;

    /// <summary>
    /// A collection of helper functions that can be imported to SQL Server.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "Required class format for SQL Server CLR integration.")]
    public partial class ClrFunctions
    {
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
                separator = @"\n";
            }

            char[] separatorChars = separator.ToCharArray();
            bool contains = String.IsNullOrEmpty(referenceSet) && String.IsNullOrEmpty(askingSet);

            if (!contains)
            {
                var r = from s in referenceSet.Split(separatorChars, StringSplitOptions.RemoveEmptyEntries)
                        select s.Trim();

                var a = from s in askingSet.Split(separatorChars, StringSplitOptions.RemoveEmptyEntries)
                        select s.Trim();

                contains = 0 < r.Intersect(a, StringComparer.OrdinalIgnoreCase).Count();
            }

            return contains;
        }
    }
}
