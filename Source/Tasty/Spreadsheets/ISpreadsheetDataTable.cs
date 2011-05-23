//-----------------------------------------------------------------------
// <copyright file="ISpreadsheetDataTable.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Spreadsheets
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface definition for spreadsheet tables.
    /// </summary>
    public interface ISpreadsheetDataTable
    {
        /// <summary>
        /// Gets the table's column collection.
        /// </summary>
        IList<ISpreadsheetDataColumn> Columns { get; }

        /// <summary>
        /// Gets the table's name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the table's row collection.
        /// </summary>
        IList<ISpreadsheetDataRow> Rows { get; }
    }
}
