//-----------------------------------------------------------------------
// <copyright file="DataSets.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
//     Adapted from code by Josip Kremenic, Copyright (c) Josip Kremenic 2009.
//     The original can be found at http://www.codeproject.com/KB/office/ReadWriteOds.aspx
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Data;

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
        public static void WriteToXlsxFile(this DataSet dataSet, string path)
        {
            new XlsxWriter().Write(dataSet, path);
        }
    }
}
