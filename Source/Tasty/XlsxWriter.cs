//-----------------------------------------------------------------------
// <copyright file="XlsxWriter.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Data;
    using System.IO;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;

    /// <summary>
    /// Implements <see cref="ISpreadsheetWriter"/> to write Excel spreadsheets.
    /// </summary>
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

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                Sheets sheets = document.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                for(int i = 0; i < dataSet.Tables.Count; i++)
                {
                    uint sheetId = (uint)(i + 1);

                    Sheet sheet = new Sheet()
                    {
                        Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = sheetId,
                        Name = dataSet.Tables[i].TableName
                    };

                    sheets.Append(sheet);


                    for (int j = 0; j < dataSet.Tables[i].Rows.Count; j++)
                    {
                        DataRow dataRow = dataSet.Tables[i].Rows[j];
                        Row row = new Row() { RowIndex = (uint)j };

                        
                    }
                }

                workbookPart.Workbook.Save();
            }
        }
    }
}
