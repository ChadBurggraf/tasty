﻿//-----------------------------------------------------------------------
// <copyright file="XlsxWriter.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;

    /// <summary>
    /// Implements <see cref="ISpreadsheetWriter"/> to write Excel spreadsheets.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class XlsxWriter : SpreadsheetWriter
    {
        /// <summary>
        /// Gets the display name of this <see cref="ISpreadsheetWriter"/> implementation.
        /// </summary>
        public override string Name
        {
            get { return "Excel"; }
        }

        /// <summary>
        /// Gets the file extension to use when saving files.
        /// </summary>
        public override string Extension
        {
            get { return ".xlsx"; }
        }

        /// <summary>
        /// Writes the given <see cref="DataSet"/> to a spreadsheet file at the given path.
        /// The path's extension will be replaced by the value of this instance's <see cref="Extension"/> property.
        /// </summary>
        /// <param name="dataSet">The <see cref="DataSet"/> to write.</param>
        /// <param name="path">The path to write to.</param>
        public override void Write(DataSet dataSet, string path)
        {
            path = CreatePath(path);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            new XlsxDocument().CreatePackage(path);

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(path, true))
            {
                WorkbookPart workbookPart = document.WorkbookPart;
                Sheets sheets = workbookPart.Workbook.Sheets;
                Stylesheet stylesheet = document.WorkbookPart.WorkbookStylesPart.Stylesheet;

                CellFormat dateFormat = new CellFormat();
                dateFormat.NumberFormatId = 14;
                dateFormat.ApplyNumberFormat = BooleanValue.FromBoolean(true);
                stylesheet.CellFormats.Append(dateFormat);
                uint dateFormatIndex = stylesheet.CellFormats.Count++;

                CellFormat timeFormat = new CellFormat();
                timeFormat.NumberFormatId = 19;
                timeFormat.ApplyNumberFormat = BooleanValue.FromBoolean(true);
                stylesheet.CellFormats.Append(timeFormat);
                uint timeFormatIndex = stylesheet.CellFormats.Count++;

                for (int i = 0; i < dataSet.Tables.Count; i++)
                {
                    WorksheetPart worksheetPart = AddWorksheet(workbookPart, sheets, dataSet.Tables[i].TableName);
                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                    Row headerRow = new Row { RowIndex = 1 };
                    sheetData.Append(headerRow);

                    for (int j = 0; j < dataSet.Tables[i].Columns.Count; j++)
                    {
                        Cell cell = new Cell()
                        {
                            CellReference = (j + 1).ToSpreadsheetColumnName() + 1,
                            DataType = CellValues.String,
                            CellValue = new CellValue(dataSet.Tables[i].Columns[j].ColumnName)
                        };

                        headerRow.Append(cell);
                    }

                    for (int j = 0; j < dataSet.Tables[i].Rows.Count; j++)
                    {
                        DataRow dataRow = dataSet.Tables[i].Rows[j];
                        uint rowNum = (uint)(j + 2);

                        Row row = new Row() { RowIndex = rowNum };
                        sheetData.Append(row);

                        for (int k = 0; k < dataRow.ItemArray.Length; k++)
                        {
                            row.Append(CreateCell(dataSet.Tables[i], dataRow, (int)rowNum, k, dateFormatIndex, timeFormatIndex));
                        }
                    }

                    worksheetPart.Worksheet.Save();
                }

                workbookPart.Workbook.Save();
            }
        }

        /// <summary>
        /// Adds a new worksheet to the given workbook.
        /// </summary>
        /// <param name="workbookPart">The <see cref="WorkbookPart"/> to add the worksheet to.</param>
        /// <param name="sheets">The workbook's <see cref="Sheets"/> collection to add a sheet reference to.</param>
        /// <param name="sheetName">The name of the sheet to add.</param>
        /// <returns>The created <see cref="WorksheetPart"/>.</returns>
        private static WorksheetPart AddWorksheet(WorkbookPart workbookPart, Sheets sheets, string sheetName)
        {
            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            string id = workbookPart.GetIdOfPart(worksheetPart);
            uint sheetId = 1;

            if (sheets.Elements<Sheet>().Count() > 0)
            {
                sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
            }

            Sheet sheet = new Sheet() { Id = id, SheetId = sheetId, Name = sheetName.Length <= 31 ? sheetName : sheetName.Substring(0, 31) };
            sheets.Append(sheet);

            workbookPart.Workbook.Save();

            return worksheetPart;
        }

        /// <summary>
        /// Creates a new cell for the given data table, row and column index.
        /// </summary>
        /// <param name="table">The data table to create the cell for.</param>
        /// <param name="row">The data row to create the cell for.</param>
        /// <param name="rowNumber">The row number to name the cell with.</param>
        /// <param name="columnIndex">The column index to create the cell for.</param>
        /// <param name="dateStyleIndex">The index of the date format style in the workbook's stylesheet.</param>
        /// <param name="timeStyleIndex">The index of the time format style in the workbook's stylesheet.</param>
        /// <returns>The created cell.</returns>
        private static Cell CreateCell(DataTable table, DataRow row, int rowNumber, int columnIndex, uint dateStyleIndex, uint timeStyleIndex)
        {
            Type columnType = table.Columns[columnIndex].DataType;
            EnumValue<CellValues> dataType;
            CellValue value;
            uint? styleIndex = null;
            bool isNull = row[columnIndex] == null || row[columnIndex] == DBNull.Value;

            if (typeof(bool).IsAssignableFrom(columnType))
            {
                dataType = new EnumValue<CellValues>(CellValues.Number);
                value = new CellValue(!isNull ? (bool)row[columnIndex] ? "1" : "0" : String.Empty);
            }
            else if (typeof(DateTime).IsAssignableFrom(columnType))
            {
                dataType = new EnumValue<CellValues>(CellValues.Number);

                if (!isNull)
                {
                    DateTime date = (DateTime)row[columnIndex];
                    value = new CellValue(date.ToOADate().ToString());

                    if (date.Date == DateTime.MinValue)
                    {
                        styleIndex = timeStyleIndex;
                    }
                    else
                    {
                        styleIndex = dateStyleIndex;
                    }
                }
                else
                {
                    value = new CellValue(String.Empty);
                    styleIndex = dateStyleIndex;
                }
            }
            else if (typeof(decimal).IsAssignableFrom(columnType) ||
                     typeof(double).IsAssignableFrom(columnType) ||
                     typeof(float).IsAssignableFrom(columnType))
            {
                dataType = new EnumValue<CellValues>(CellValues.Number);
                value = new CellValue(!isNull ? String.Format(CultureInfo.InvariantCulture, "{0:N2}", row[columnIndex]) : String.Empty);
            }
            else if (typeof(int).IsAssignableFrom(columnType) ||
                     typeof(long).IsAssignableFrom(columnType))
            {
                dataType = new EnumValue<CellValues>(CellValues.Number);
                value = new CellValue(!isNull ? row[columnIndex].ToString() : String.Empty);
            }
            else
            {
                dataType = new EnumValue<CellValues>(CellValues.String);
                value = new CellValue(!isNull ? row[columnIndex].ToString() : String.Empty);
            }

            Cell cell = new Cell()
            {
                CellReference = (columnIndex + 1).ToSpreadsheetColumnName() + rowNumber,
                DataType = dataType,
                CellValue = value
            };

            if (styleIndex.HasValue)
            {
                cell.StyleIndex = styleIndex;
            }

            return cell;
        }
    }
}
