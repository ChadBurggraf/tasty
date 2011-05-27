//-----------------------------------------------------------------------
// <copyright file="SpreadsheetDataRow.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Spreadsheets
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// Implements <see cref="ISpreadsheetDataRow"/> for <see cref="DataRow"/>s.
    /// </summary>
    public class SpreadsheetDataRow : ISpreadsheetDataRow
    {
        private DataRow row;

        /// <summary>
        /// Initializes a new instance of the SpreadsheetDataRow class.
        /// </summary>
        /// <param name="row">The <see cref="DataRow"/> to wrap.</param>
        public SpreadsheetDataRow(DataRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException("row", "row cannot be null.");
            }

            this.row = row;
        }

        /// <summary>
        /// Gets the value of the item in the given column for this row.
        /// </summary>
        /// <param name="columnIndex">The index of the column to get the item value for.</param>
        /// <returns>The value of the item in the given column.</returns>
        public object this[int columnIndex]
        {
            get { return this.row[columnIndex]; }
        }

        /// <summary>
        /// Gets the value of the item in the given column for this row.
        /// </summary>
        /// <param name="columnName">The name of the column to get the item value for.</param>
        /// <returns>The value of the item in the given column.</returns>
        public object this[string columnName]
        {
            get { return this.row[columnName]; }
        }
    }
}
