﻿//-----------------------------------------------------------------------
// <copyright file="ISpreadsheetDataColumn.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Spreadsheets
{
    using System;

    /// <summary>
    /// Interface definition for spreadsheet columns.
    /// </summary>
    public interface ISpreadsheetDataColumn
    {
        /// <summary>
        /// Gets the type of the column's data.
        /// </summary>
        Type DataType { get; }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        string Name { get; }
    }
}
