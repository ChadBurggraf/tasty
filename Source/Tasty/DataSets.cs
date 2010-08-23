//-----------------------------------------------------------------------
// <copyright file="DataSets.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// Provides extensions and helpers for <see cref="DataSet"/>s.
    /// </summary>
    public static class DataSets
    {
        /// <summary>
        /// Writes the <see cref="DataSet"/> to an OpenDocument spreadsheet (.ods file).
        /// </summary>
        /// <param name="dataSet">The <see cref="DataSet"/> to write.</param>
        /// <param name="path">The path to write to.</param>
        public static void WriteToOdsFile(this DataSet dataSet, string path)
        {
            new OdsWriter().Write(dataSet, path);
        }

        /// <summary>
        /// Writes the <see cref="DataSet"/> to an Excel spreadsheet (.xlsx file).
        /// </summary>
        /// <param name="dataSet">The <see cref="DataSet"/> to write.</param>
        /// <param name="path">The path to write to.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public static void WriteToXlsxFile(this DataSet dataSet, string path)
        {
            new XlsxWriter().Write(dataSet, path);
        }
    }
}
