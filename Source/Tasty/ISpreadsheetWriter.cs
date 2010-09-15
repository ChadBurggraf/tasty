//-----------------------------------------------------------------------
// <copyright file="ISpreadsheetWriter.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Data;

    /// <summary>
    /// Interface definition for spreadhseet writers.
    /// </summary>
    public interface ISpreadsheetWriter
    {
        /// <summary>
        /// Gets the display name of this <see cref="ISpreadsheetWriter"/> implementation.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the file extension to use when saving files.
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// Writes the given <see cref="DataSet"/> to a spreadsheet file at the given path.
        /// The path's extension will be replaced by the value of this instance's <see cref="Extension"/> property.
        /// </summary>
        /// <param name="dataSet">The <see cref="DataSet"/> to write.</param>
        /// <param name="path">The path to write to.</param>
        void Write(DataSet dataSet, string path);
    }
}
