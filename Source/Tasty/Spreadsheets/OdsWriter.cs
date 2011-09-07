//-----------------------------------------------------------------------
// <copyright file="OdsWriter.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Spreadsheets
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using Ionic.Zip;

    /// <summary>
    /// Implements <see cref="ISpreadsheetWriter"/> to write OpenDocument spreadsheets.
    /// </summary>
    public class OdsWriter : SpreadsheetWriter
    {
        private const string DateCellStyleName = "ce1";
        private const string OdsTemplateName = "Tasty.Template.ods";
        private const string TableStyleName = "ta1";
        private const string TimeCellStyleName = "ce2";
        private static ReadOnlyDictionary<string, string> ns = CreateNamespaceLookup();

        /// <summary>
        /// Gets the display name of this <see cref="ISpreadsheetWriter"/> implementation.
        /// </summary>
        public override string Name
        {
            get { return "OpenDocument"; }
        }

        /// <summary>
        /// Gets the file extension to use when saving files.
        /// </summary>
        public override string Extension
        {
            get { return ".ods"; }
        }

        /// <summary>
        /// Writes the given <see cref="ISpreadsheetDataSet"/> to a spreadsheet file at the given path.
        /// The path's extension will be replaced by the value of this instance's <see cref="Extension"/> property.
        /// </summary>
        /// <param name="dataSet">The <see cref="ISpreadsheetDataSet"/> to write.</param>
        /// <param name="path">The path to write to.</param>
        public override void Write(ISpreadsheetDataSet dataSet, string path)
        {
            string directory = Path.GetDirectoryName(path);
            string tempPath = Path.Combine(directory, Path.GetRandomFileName());

            if (!String.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            try
            {
                using (XmlWriter xw = XmlWriter.Create(tempPath, new XmlWriterSettings() { Indent = true }))
                {
                    xw.WriteStartElement("office", "document-content", ns["office"]);

                    // Declare the namespaces.
                    foreach (string key in ns.Keys)
                    {
                        xw.WriteAttributeString("xmlns", key, null, ns[key]);
                    }

                    // Font faces.
                    xw.WriteStartElement("font-face-decls", ns["office"]);
                    WriteFontFace(xw, "Arial", "Arial", "swiss", "variable");
                    WriteFontFace(xw, "Arial Unicode MS", "'Arial Unicode MS'", "system", "variable");
                    WriteFontFace(xw, "Tahoma", "Tahoma", "system", "variable");
                    xw.WriteEndElement();

                    // Cell and table styles.
                    xw.WriteStartElement("automatic-styles", ns["office"]);
                    WriteCellStyle(xw, "ce1", "table-cell", "Default", "N37");
                    WriteCellStyle(xw, "ce2", "table-cell", "Default", "N5042");
                    WriteTableStyle(xw, "ta1", "table", "Default", "true", "lr-tb");
                    xw.WriteEndElement();

                    // Main body.
                    xw.WriteStartElement("body", ns["office"]);
                    xw.WriteStartElement("spreadsheet", ns["office"]);

                    foreach (ISpreadsheetDataTable table in dataSet.Tables)
                    {
                        WriteTableStartElement(xw, table.Name, "ta1", "false");
                        WriteTableColumnDefinition(xw, "co1", "Default", table.Columns.Count.ToString(CultureInfo.InvariantCulture));

                        // Header.
                        xw.WriteStartElement("table-row", ns["table"]);

                        foreach (ISpreadsheetDataColumn column in table.Columns)
                        {
                            WriteCell(xw, typeof(string), column.Name);
                        }

                        xw.WriteEndElement();

                        // Content rows.
                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            xw.WriteStartElement("table-row", ns["table"]);

                            for (int j = 0; j < table.Columns.Count; j++)
                            {
                                WriteCell(xw, table.Columns[j].DataType, table.Rows[i][table.Columns[j].Name]);
                            }

                            xw.WriteEndElement();

                            if (i % 100 == 0)
                            {
                                xw.Flush();
                            }
                        }

                        xw.WriteEndElement();
                        xw.Flush();
                    }

                    xw.WriteEndElement();
                    xw.WriteEndElement();

                    xw.WriteEndElement();
                }

                SavePackage(tempPath, CreatePath(path));
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        /// <summary>
        /// Creates a namespace lookup dictionary for use by the writer.
        /// </summary>
        /// <returns>A namespace lookup dictionary.</returns>
        private static ReadOnlyDictionary<string, string> CreateNamespaceLookup()
        {
            Dictionary<string, string> n = new Dictionary<string, string>();
            n["office"] = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
            n["table"] = "urn:oasis:names:tc:opendocument:xmlns:table:1.0";
            n["style"] = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";
            n["text"] = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
            n["svg"] = "urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0";
            return new ReadOnlyDictionary<string, string>(n);
        }

        /// <summary>
        /// Saves an ODS package, using the content document at the given path, to the given output path.
        /// </summary>
        /// <param name="contentXmlPath">The path of the content document.</param>
        /// <param name="outputPath">The output path to save the package to.</param>
        private static void SavePackage(string contentXmlPath, string outputPath)
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            using (Stream templateStream = Assembly.GetAssembly(typeof(DataSets)).GetManifestResourceStream(OdsTemplateName))
            {
                using (ZipFile odsFile = ZipFile.Read(templateStream))
                {
                    odsFile.RemoveEntry("content.xml");

                    using (FileStream contentStream = File.OpenRead(contentXmlPath))
                    {
                        odsFile.AddEntry("content.xml", contentStream);
                        odsFile.Save(outputPath);
                    }
                }
            }
        }

        /// <summary>
        /// Writes a cell.
        /// </summary>
        /// <param name="xw">The <see cref="XmlWriter"/> to write to.</param>
        /// <param name="columnType">The cell's column type.</param>
        /// <param name="value">The cell's value.</param>
        private static void WriteCell(XmlWriter xw, Type columnType, object value)
        {
            string cellStyleName = String.Empty,
                cellText = String.Empty,
                cellValue = String.Empty,
                cellValueName = "value",
                cellValueType = String.Empty;

            xw.WriteStartElement("table-cell", ns["table"]);

            if (value != null && value != DBNull.Value)
            {
                if (typeof(bool).IsAssignableFrom(columnType))
                {
                    cellText = cellValue = Convert.ToBoolean(value, CultureInfo.InvariantCulture) ? "1" : "0";
                    cellValueType = "float";
                }
                else if (typeof(DateTime).IsAssignableFrom(columnType))
                {
                    DateTime dateValue = Convert.ToDateTime(value, CultureInfo.InvariantCulture);

                    if (dateValue.Date == DateTime.MinValue)
                    {
                        cellStyleName = TimeCellStyleName;
                        cellText = String.Format(CultureInfo.InvariantCulture, "{0:hh}:{0:mm} {0:tt}", dateValue);
                        cellValue = String.Format(CultureInfo.InvariantCulture, "PT{0:HH}H{0:mm}M{0:ss}S", dateValue);
                        cellValueName = "time-value";
                        cellValueType = "time";
                    }
                    else
                    {
                        cellStyleName = DateCellStyleName;
                        cellText = String.Format(CultureInfo.InvariantCulture, "{0:d}", dateValue);
                        cellValue = String.Format(CultureInfo.InvariantCulture, "{0:yyyy}-{0:MM}-{0:dd}T{0:HH}:{0:mm}:{0:ss}", dateValue);
                        cellValueName = "date-value";
                        cellValueType = "date";
                    }
                }
                else if (typeof(decimal).IsAssignableFrom(columnType) ||
                         typeof(double).IsAssignableFrom(columnType) ||
                         typeof(float).IsAssignableFrom(columnType))
                {
                    cellText = cellValue = String.Format(CultureInfo.InvariantCulture, "{0:N2}", value);
                    cellValueType = "float";
                }
                else if (typeof(int).IsAssignableFrom(columnType) ||
                         typeof(long).IsAssignableFrom(columnType))
                {
                    cellText = cellValue = value.ToString();
                    cellValueType = "float";
                }
                else
                {
                    cellText = value.ToString();
                    cellValueType = "string";
                }
            }

            if (!String.IsNullOrEmpty(cellStyleName))
            {
                xw.WriteAttributeString("style-name", ns["table"], cellStyleName.HtmlAttributeEncode());
            }

            if (!String.IsNullOrEmpty(cellValueType))
            {
                xw.WriteAttributeString("value-type", ns["office"], cellValueType.HtmlAttributeEncode());
            }

            if (!String.IsNullOrEmpty(cellValue))
            {
                xw.WriteAttributeString(XmlConvert.EncodeName(cellValueName), ns["office"], cellValue.HtmlAttributeEncode());
            }

            if (!String.IsNullOrEmpty(cellText))
            {
                // WriteString encodes the value.
                xw.WriteStartElement("p", ns["text"]);
                xw.WriteString(cellText);
                xw.WriteEndElement();
            }

            xw.WriteEndElement();
        }

        /// <summary>
        /// Writes a preamble cell style element.
        /// </summary>
        /// <param name="xw">The <see cref="XmlWriter"/> to write to.</param>
        /// <param name="name">The style's name.</param>
        /// <param name="family">The style's family.</param>
        /// <param name="parentStyleName">The style's parent style name.</param>
        /// <param name="dataStyleName">The style's data style name.</param>
        private static void WriteCellStyle(XmlWriter xw, string name, string family, string parentStyleName, string dataStyleName)
        {
            xw.WriteStartElement("style", ns["style"]);
            xw.WriteAttributeString("name", ns["style"], name.HtmlAttributeEncode());
            xw.WriteAttributeString("family", ns["style"], family.HtmlAttributeEncode());
            xw.WriteAttributeString("parent-style-name", ns["style"], parentStyleName.HtmlAttributeEncode());
            xw.WriteAttributeString("data-style-name-", ns["style"], dataStyleName.HtmlAttributeEncode());
            xw.WriteEndElement();
        }

        /// <summary>
        /// Writes a preamble font face element.
        /// </summary>
        /// <param name="xw">The <see cref="XmlWriter"/> to write to.</param>
        /// <param name="name">The font face's name.</param>
        /// <param name="fontFamily">The font face's font family.</param>
        /// <param name="fontFamilyGeneric">The font face's generic font family.</param>
        /// <param name="fontPitch">The font face's pitch.</param>
        private static void WriteFontFace(XmlWriter xw, string name, string fontFamily, string fontFamilyGeneric, string fontPitch)
        {
            xw.WriteStartElement("font-face", ns["style"]);
            xw.WriteAttributeString("name", ns["style"], name.HtmlAttributeEncode());
            xw.WriteAttributeString("font-family", ns["svg"], fontFamily.HtmlAttributeEncode());
            xw.WriteAttributeString("font-family-generic", ns["style"], fontFamilyGeneric.HtmlAttributeEncode());
            xw.WriteAttributeString("font-pitch", ns["style"], fontPitch.HtmlAttributeEncode());
            xw.WriteEndElement();
        }

        /// <summary>
        /// Writes a table column definition.
        /// </summary>
        /// <param name="xw">The <see cref="XmlWriter"/> to write to.</param>
        /// <param name="styleName">The column's style name.</param>
        /// <param name="defaultCellStyleName">The column's default cell style name.</param>
        /// <param name="numberColumnsRepeated">The number of columns being defined.</param>
        private static void WriteTableColumnDefinition(XmlWriter xw, string styleName, string defaultCellStyleName, string numberColumnsRepeated)
        {
            xw.WriteStartElement("table-column", ns["table"]);
            xw.WriteAttributeString("style-name", ns["table"], styleName.HtmlAttributeEncode());
            xw.WriteAttributeString("default-cell-style-name", ns["table"], defaultCellStyleName.HtmlAttributeEncode());
            xw.WriteAttributeString("number-columns-repeated", ns["table"], numberColumnsRepeated.HtmlAttributeEncode());
            xw.WriteEndElement();
        }

        /// <summary>
        /// Writes a table's start element.
        /// </summary>
        /// <param name="xw">The <see cref="XmlWriter"/> to write to.</param>
        /// <param name="name">The table's name.</param>
        /// <param name="styleName">The table's style name.</param>
        /// <param name="print">The table's print value.</param>
        private static void WriteTableStartElement(XmlWriter xw, string name, string styleName, string print)
        {
            xw.WriteStartElement("table", ns["table"]);
            xw.WriteAttributeString("name", ns["table"], name.HtmlAttributeEncode());
            xw.WriteAttributeString("style-name", ns["table"], styleName.HtmlAttributeEncode());
            xw.WriteAttributeString("print", ns["table"], print.HtmlAttributeEncode());
        }

        /// <summary>
        /// Writes a preamble table style element.
        /// </summary>
        /// <param name="xw">The <see cref="XmlWriter"/> to write to.</param>
        /// <param name="name">The style's name.</param>
        /// <param name="family">The style's family.</param>
        /// <param name="masterPageName">The style's master page name.</param>
        /// <param name="display">The style's display value.</param>
        /// <param name="writingMode">The style's writing mode.</param>
        private static void WriteTableStyle(XmlWriter xw, string name, string family, string masterPageName, string display, string writingMode)
        {
            xw.WriteStartElement("style", ns["style"]);
            xw.WriteAttributeString("name", ns["style"], name.HtmlAttributeEncode());
            xw.WriteAttributeString("family", ns["style"], family.HtmlAttributeEncode());
            xw.WriteAttributeString("master-page-name", ns["style"], masterPageName.HtmlAttributeEncode());
            xw.WriteStartElement("table-properties", ns["style"]);
            xw.WriteAttributeString("display", ns["table"], display.HtmlAttributeEncode());
            xw.WriteAttributeString("writing-mode", ns["style"], writingMode.HtmlAttributeEncode());
            xw.WriteEndElement();
            xw.WriteEndElement();
        }
    }
}
