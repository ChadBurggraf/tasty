//-----------------------------------------------------------------------
// <copyright file="ISpreadsheetDataRow.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Spreadsheets
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface definition for spreadsheet rows.
    /// </summary>
    public interface ISpreadsheetDataRow
    {
        /// <summary>
        /// Gets the row's item collection.
        /// </summary>
        IEnumerable<object> Items { get; }

        /// <summary>
        /// Gets the value of the item in the given column for this row.
        /// </summary>
        /// <param name="columnIndex">The index of the column to get the item value for.</param>
        /// <returns>The value of the item in the given column.</returns>
        object this[int columnIndex] 
        { 
            get; 
        }
    
        /// <summary>
        /// Gets the value of the item in the given column for this row.
        /// </summary>
        /// <param name="columnName">The name of the column to get the item value for.</param>
        /// <returns>The value of the item in the given column.</returns>
        object this[string columnName] 
        { 
            get; 
        }
    }
}
