//-----------------------------------------------------------------------
// <copyright file="SpreadsheetDataSet.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Spreadsheets
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// Implements <see cref="ISpreadsheetDataSet"/> for <see cref="DataSet"/>s.
    /// </summary>
    public class SpreadsheetDataSet : ISpreadsheetDataSet
    {
        /// <summary>
        /// Initializes a new instance of the SpreadsheetDataSet class.
        /// </summary>
        /// <param name="dataSet">The <see cref="DataSet"/> to wrap.</param>
        public SpreadsheetDataSet(DataSet dataSet)
        {
            List<ISpreadsheetDataTable> tables = new List<ISpreadsheetDataTable>();

            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                foreach (DataTable table in dataSet.Tables)
                {
                    tables.Add(new SpreadsheetDataTable(table));
                }
            }

            this.Tables = tables;
        }

        /// <summary>
        /// Gets the set's table collection.
        /// </summary>
        public IList<ISpreadsheetDataTable> Tables { get; private set; }
    }
}
