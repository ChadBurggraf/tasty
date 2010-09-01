//-----------------------------------------------------------------------
// <copyright file="SpreadsheetWriter.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Base class for <see cref="ISpreadsheetWriter"/> implementors.
    /// </summary>
    public abstract class SpreadsheetWriter : ISpreadsheetWriter
    {
        /// <summary>
        /// Gets the display name of this <see cref="ISpreadsheetWriter"/> implementation.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the file extension to use when saving files.
        /// </summary>
        public abstract string Extension { get; }

        /// <summary>
        /// Writes the given <see cref="DataSet"/> to a spreadsheet file at the given path.
        /// The path's extension will be replaced by the value of this instance's <see cref="Extension"/> property.
        /// </summary>
        /// <param name="dataSet">The <see cref="DataSet"/> to write.</param>
        /// <param name="path">The path to write to.</param>
        public abstract void Write(DataSet dataSet, string path);

        /// <summary>
        /// Gets the cell at the given location as a display string based on the type of the <see cref="DataColumn"/>
        /// it resides in.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to get the cell value from.</param>
        /// <param name="rowIndex">The index of the row to get the cell value form.</param>
        /// <param name="columnIndex">The index of the column to get the cell value from.</param>
        /// <returns>The specified cell value as a string.</returns>
        protected static string GetCellValueAsString(DataTable table, int rowIndex, int columnIndex)
        {
            string value = String.Empty;
            object obj = table.Rows[rowIndex][columnIndex];

            if (obj != null && obj != DBNull.Value)
            {
                Type type = table.Columns[columnIndex].DataType;

                if (typeof(bool).IsAssignableFrom(type))
                {
                    value = (bool)obj ? "1" : "0";
                }
                else if (typeof(DateTime).IsAssignableFrom(type))
                {
                    DateTime dateValue = (DateTime)obj;

                    if (dateValue.Date == DateTime.MinValue)
                    {
                        value = String.Format(CultureInfo.InvariantCulture, "{0:hh}:{0:mm} {0:tt}", dateValue);
                    }
                    else
                    {
                        value = String.Format(CultureInfo.InvariantCulture, "{0:d}", dateValue);
                    }
                }
                else if (typeof(decimal).IsAssignableFrom(type) || typeof(double).IsAssignableFrom(type))
                {
                    value = String.Format(CultureInfo.InvariantCulture, "{0:N2}", obj);
                }
                else if (typeof(int).IsAssignableFrom(type) || typeof(long).IsAssignableFrom(type))
                {
                    value = String.Format(CultureInfo.InvariantCulture, "{0}", obj);
                }
                else if (typeof(TimeSpan).IsAssignableFrom(type))
                {
                    value = String.Format(CultureInfo.InvariantCulture, "{0:hh}:{0:mm} {0:tt}", DateTime.MinValue.Add((TimeSpan)obj));
                }
                else
                {
                    value = String.Format(CultureInfo.InvariantCulture, "{0}", obj);
                }
            }

            return value;
        }

        /// <summary>
        /// Gets a path for this instance from the given path, replacing the given file extension as necessary.
        /// </summary>
        /// <param name="path">The path to create this instance's path from.</param>
        /// <returns>The created path.</returns>
        protected string CreatePath(string path)
        {
            return Path.Combine(Path.GetDirectoryName(path), String.Concat(Path.GetFileNameWithoutExtension(path), this.Extension));
        }
    }
}
