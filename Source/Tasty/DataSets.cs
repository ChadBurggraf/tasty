//-----------------------------------------------------------------------
// <copyright file="DataSets.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Tasty.Spreadsheets;

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
            new SpreadsheetDataSet(dataSet).WriteToOdsFile(path);
        }

        /// <summary>
        /// Writes the <see cref="ISpreadsheetDataSet"/> to an OpenDocument spreadsheet (.ods file).
        /// </summary>
        /// <param name="dataSet">The <see cref="ISpreadsheetDataSet"/> to write.</param>
        /// <param name="path">The path to write to.</param>
        public static void WriteToOdsFile(this ISpreadsheetDataSet dataSet, string path)
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
            new SpreadsheetDataSet(dataSet).WriteToXlsxFile(path);
        }

        /// <summary>
        /// Writes the <see cref="ISpreadsheetDataSet"/> to an Excel spreadsheet (.xlsx file).
        /// </summary>
        /// <param name="dataSet">The <see cref="ISpreadsheetDataSet"/> to write.</param>
        /// <param name="path">The path to write to.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public static void WriteToXlsxFile(this ISpreadsheetDataSet dataSet, string path)
        {
            new XlsxWriter().Write(dataSet, path);
        }
    }
}
