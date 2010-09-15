//-----------------------------------------------------------------------
// <copyright file="XlsxWriter.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
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

            using (SpreadsheetDocument document = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                Sheets sheets = workbookPart.Workbook.AppendChild<Sheets>(new Sheets());

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
                            DataType = CellValues.InlineString,
                            InlineString = new InlineString(new Text(dataSet.Tables[i].Columns[j].ColumnName))
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
                            Cell cell = new Cell()
                            {
                                CellReference = (k + 1).ToSpreadsheetColumnName() + rowNum,
                                DataType = CellValues.InlineString,
                                InlineString = new InlineString(new Text(dataRow[k] != null ? dataRow[k].ToString() : String.Empty))
                            };

                            row.Append(cell);
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

            Sheet sheet = new Sheet() { Id = id, SheetId = sheetId, Name = sheetName };
            sheets.Append(sheet);

            workbookPart.Workbook.Save();

            return worksheetPart;
        }
    }
}
