//-----------------------------------------------------------------------
// <copyright file="SpreadsheetDataColumn.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Spreadsheets
{
    using System;
    using System.Data;

    /// <summary>
    /// Implements <see cref="ISpreadsheetDataColumn"/> for <see cref="DataColumn"/>s.
    /// </summary>
    public class SpreadsheetDataColumn : ISpreadsheetDataColumn
    {
        private DataColumn column;

        /// <summary>
        /// Initializes a new instance of the SpreadsheetDataColumn class.
        /// </summary>
        /// <param name="column">The <see cref="DataColumn"/> to wrap.</param>
        public SpreadsheetDataColumn(DataColumn column)
        {
            if (column == null)
            {
                throw new ArgumentNullException("column", "column cannot be null.");
            }

            this.column = column;
        }

        /// <summary>
        /// Gets the type of the column's data.
        /// </summary>
        public Type DataType
        {
            get { return this.column.DataType; }
        }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        public string Name
        {
            get { return this.column.ColumnName; }
        }
    }
}
