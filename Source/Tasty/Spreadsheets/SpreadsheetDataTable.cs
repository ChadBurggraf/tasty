//-----------------------------------------------------------------------
// <copyright file="SpreadsheetDataTable.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Spreadsheets
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// Implements <see cref="ISpreadsheetDataTable"/> for <see cref="DataTable"/>s.
    /// </summary>
    public class SpreadsheetDataTable : ISpreadsheetDataTable
    {
        /// <summary>
        /// Initializes a new instance of the SpreadsheetDataTable class.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to wrap.</param>
        public SpreadsheetDataTable(DataTable table)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table", "table cannot be null.");
            }

            List<ISpreadsheetDataColumn> columns = new List<ISpreadsheetDataColumn>();
            List<ISpreadsheetDataRow> rows = new List<ISpreadsheetDataRow>();

            foreach (DataColumn column in table.Columns)
            {
                columns.Add(new SpreadsheetDataColumn(column));
            }

            foreach (DataRow row in table.Rows)
            {
                rows.Add(new SpreadsheetDataRow(row));
            }

            this.Columns = columns;
            this.Name = table.TableName;
            this.Rows = rows;
        }

        /// <summary>
        /// Gets the table's column collection.
        /// </summary>
        public IList<ISpreadsheetDataColumn> Columns { get; private set; }

        /// <summary>
        /// Gets the table's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the table's row collection.
        /// </summary>
        public IList<ISpreadsheetDataRow> Rows { get; private set; }
    }
}
