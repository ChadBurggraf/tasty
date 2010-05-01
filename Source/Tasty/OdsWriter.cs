//-----------------------------------------------------------------------
// <copyright file="OdsWriter.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
//     Adapted from code by Josip Kremenic, Copyright (c) Josip Kremenic 2009.
//     The original can be found at http://www.codeproject.com/KB/office/ReadWriteOds.aspx
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using Ionic.Zip;

    /// <summary>
    /// Implements <see cref="ISpreadsheetWriter"/> to write OpenDocument spreadsheets.
    /// </summary>
    public class OdsWriter : SpreadsheetWriter
    {
        /// <summary>
        /// Gets the name of the style to apply to date cells.
        /// </summary>
        private const string DateCellStyleName = "ce1";

        /// <summary>
        /// The name of the embedded ODS template file.
        /// </summary>
        private const string OdsTemplateName = "Tasty.Template.ods";

        /// <summary>
        /// Gets the name of the style to apply to tables.
        /// </summary>
        private const string TableStyleName = "ta1";

        /// <summary>
        /// Gets the name of the style to apply to time cells.
        /// </summary>
        private const string TimeCellStyleName = "ce2";

        /// <summary>
        /// A collection of ODF namespaces and their prefixes.
        /// </summary>
        private static string[,] odfNamespaces = new string[,]
        {
            { "table", "urn:oasis:names:tc:opendocument:xmlns:table:1.0" },
            { "office", "urn:oasis:names:tc:opendocument:xmlns:office:1.0" },
            { "style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0" },
            { "text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0" },            
            { "draw", "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" },
            { "fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" },
            { "dc", "http://purl.org/dc/elements/1.1/" },
            { "meta", "urn:oasis:names:tc:opendocument:xmlns:meta:1.0" },
            { "number", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0" },
            { "presentation", "urn:oasis:names:tc:opendocument:xmlns:presentation:1.0" },
            { "svg", "urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" },
            { "chart", "urn:oasis:names:tc:opendocument:xmlns:chart:1.0" },
            { "dr3d", "urn:oasis:names:tc:opendocument:xmlns:dr3d:1.0" },
            { "math", "http://www.w3.org/1998/Math/MathML" },
            { "form", "urn:oasis:names:tc:opendocument:xmlns:form:1.0" },
            { "script", "urn:oasis:names:tc:opendocument:xmlns:script:1.0" },
            { "ooo", "http://openoffice.org/2004/office" },
            { "ooow", "http://openoffice.org/2004/writer" },
            { "oooc", "http://openoffice.org/2004/calc" },
            { "dom", "http://www.w3.org/2001/xml-events" },
            { "xforms", "http://www.w3.org/2002/xforms" },
            { "xsd", "http://www.w3.org/2001/XMLSchema" },
            { "xsi", "http://www.w3.org/2001/XMLSchema-instance" },
            { "rpt", "http://openoffice.org/2005/report" },
            { "of", "urn:oasis:names:tc:opendocument:xmlns:of:1.2" },
            { "rdfa", "http://docs.oasis-open.org/opendocument/meta/rdfa#" },
            { "config", "urn:oasis:names:tc:opendocument:xmlns:config:1.0" }
        };

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
        /// Writes the given <see cref="DataSet"/> to a spreadsheet file at the given path.
        /// The path's extension will be replaced by the value of this instance's <see cref="Extension"/> property.
        /// </summary>
        /// <param name="dataSet">The <see cref="DataSet"/> to write.</param>
        /// <param name="path">The path to write to.</param>
        public override void Write(DataSet dataSet, string path)
        {
            XmlDocument document = new XmlDocument();
            LoadDocumentWithTemplate(document);

            XmlNamespaceManager namespaceManager = CreateNamespaceManager(document);
            XmlNode sheetsNode = GetCleanSheetsRoot(document, namespaceManager);

            string tableUri = namespaceManager.LookupNamespace("table");
            string officeUri = namespaceManager.LookupNamespace("office");
            string textUri = namespaceManager.LookupNamespace("text");

            foreach (DataTable table in dataSet.Tables)
            {
                XmlNode sheetNode = document.CreateElement("table:table", tableUri);

                XmlAttribute sheetNameAttribute = document.CreateAttribute("table:name", tableUri);
                sheetNameAttribute.Value = table.TableName;
                sheetNode.Attributes.Append(sheetNameAttribute);

                XmlAttribute sheetStyleAttribute = document.CreateAttribute("table:style-name", tableUri);
                sheetStyleAttribute.Value = TableStyleName;
                sheetNode.Attributes.Append(sheetStyleAttribute);

                XmlNode columnDefNode = document.CreateElement("table:table-column", tableUri);

                XmlAttribute columnCountAttribute = document.CreateAttribute("table:number-columns-repeated", tableUri);
                columnCountAttribute.Value = table.Columns.Count.ToString(CultureInfo.InvariantCulture);
                columnDefNode.Attributes.Append(columnCountAttribute);

                sheetNode.AppendChild(columnDefNode);

                XmlNode headerNode = document.CreateElement("table:table-row", tableUri);

                foreach (DataColumn column in table.Columns)
                {
                    XmlNode cellNode = document.CreateElement("table:table-cell", tableUri);
                    SetCellValue(document, cellNode, officeUri, tableUri, textUri, typeof(string), column.ColumnName);
                    headerNode.AppendChild(cellNode);
                }

                sheetNode.AppendChild(headerNode);

                foreach (DataRow row in table.Rows)
                {
                    XmlNode rowNode = document.CreateElement("table:table-row", tableUri);

                    for (int i = 0; i < row.ItemArray.Length; i++)
                    {
                        XmlNode cellNode = document.CreateElement("table:table-cell", tableUri);
                        SetCellValue(document, cellNode, officeUri, tableUri, textUri, table.Columns[i].DataType, row[i]);
                        rowNode.AppendChild(cellNode);
                    }

                    sheetNode.AppendChild(rowNode);
                }

                sheetsNode.AppendChild(sheetNode);
            }

            SaveDocumentWithTemplate(document, CreatePath(path));
        }

        /// <summary>
        /// Creates an <see cref="XmlNamespaceManager"/> for the given document using
        /// the ODF namespace set.
        /// </summary>
        /// <param name="document">The <see cref="XmlDocument"/> to create the <see cref="XmlNamespaceManager"/> for.</param>
        /// <returns>An <see cref="XmlNamespaceManager"/>.</returns>
        private static XmlNamespaceManager CreateNamespaceManager(XmlDocument document)
        {
            XmlNamespaceManager mgr = new XmlNamespaceManager(document.NameTable);

            for (int i = 0; i < odfNamespaces.GetLength(0); i++)
            {
                mgr.AddNamespace(odfNamespaces[i, 0], odfNamespaces[i, 1]);
            }

            return mgr;
        }

        /// <summary>
        /// Gets the sheets node from the given <see cref="XmlDocument"/>, cleaned of all of its child elements.
        /// </summary>
        /// <param name="document">The <see cref="XmlDocument"/> to get the node from.</param>
        /// <param name="namespaceManager">The <see cref="XmlNamespaceManager"/> to use when searching the document.</param>
        /// <returns>A clean sheets node.</returns>
        private static XmlNode GetCleanSheetsRoot(XmlDocument document, XmlNamespaceManager namespaceManager)
        {
            XmlNodeList tables = document.SelectNodes("/office:document-content/office:body/office:spreadsheet/table:table", namespaceManager);
            XmlNode sheetsRoot = tables[0].ParentNode;

            foreach (XmlNode table in tables)
            {
                sheetsRoot.RemoveChild(table);
            }

            return sheetsRoot;
        }

        /// <summary>
        /// Loads the given <see cref="XmlDocument"/> with the contents portion of the ODS template.
        /// </summary>
        /// <param name="document">The <see cref="XmlDocument"/> to load.</param>
        private static void LoadDocumentWithTemplate(XmlDocument document)
        {
            using (Stream templateStream = Assembly.GetAssembly(typeof(DataSets)).GetManifestResourceStream(OdsTemplateName))
            {
                using (ZipFile odsFile = ZipFile.Read(templateStream))
                {
                    ZipEntry contentsEntry = odsFile["content.xml"];

                    using (MemoryStream contentsStream = new MemoryStream())
                    {
                        contentsEntry.Extract(contentsStream);
                        contentsStream.Seek(0, SeekOrigin.Begin);

                        document.Load(contentsStream);
                    }
                }
            }
        }

        /// <summary>
        /// Saves the given <see cref="XmlDocument"/> as the contents of the ODS template and
        /// write the final ODS file to the given path.
        /// </summary>
        /// <param name="document">The <see cref="XmlDocument"/> to save as ODS contents.</param>
        /// <param name="path">The path to write the ODS file to.</param>
        private static void SaveDocumentWithTemplate(XmlDocument document, string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (Stream templateStream = Assembly.GetAssembly(typeof(DataSets)).GetManifestResourceStream(OdsTemplateName))
            {
                using (ZipFile odsFile = ZipFile.Read(templateStream))
                {
                    odsFile.RemoveEntry("content.xml");

                    using (MemoryStream contentsStream = new MemoryStream())
                    {
                        document.Save(contentsStream);
                        contentsStream.Seek(0, SeekOrigin.Begin);
                        odsFile.AddEntry("content.xml", contentsStream);
                        odsFile.Save(path);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the value of the given cell <see cref="XmlNode"/>.
        /// </summary>
        /// <param name="document">The <see cref="XmlDocument"/> the given cell belongs to.</param>
        /// <param name="cellNode">The cell <see cref="XmlNode"/> to set the value of.</param>
        /// <param name="officeUri">The office namespace URI to use.</param>
        /// <param name="tableUri">The table namespace URI to use.</param>
        /// <param name="textUri">The text namespace URI to use.</param>
        /// <param name="columnType">The type of the column the cell belongs to.</param>
        /// <param name="value">The value to set.</param>
        private static void SetCellValue(XmlDocument document, XmlNode cellNode, string officeUri, string tableUri, string textUri, Type columnType, object value)
        {
            string cellStyleName = String.Empty,
                cellText = String.Empty,
                cellValue = String.Empty,
                cellValueName = "value",
                cellValueType = String.Empty;

            if (value != null)
            {
                if (typeof(bool).IsAssignableFrom(columnType))
                {
                    cellText = cellValue = (bool)value ? "1" : "0";
                    cellValueType = "float";
                }
                else if (typeof(DateTime).IsAssignableFrom(columnType))
                {
                    DateTime dateValue = (DateTime)value;

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
                XmlAttribute cellStyleNameAttribute = document.CreateAttribute("table:style-name", tableUri);
                cellStyleNameAttribute.Value = cellStyleName;
                cellNode.Attributes.Append(cellStyleNameAttribute);
            }

            if (!String.IsNullOrEmpty(cellValueType))
            {
                XmlAttribute cellValueTypeAttribute = document.CreateAttribute("office:value-type", officeUri);
                cellValueTypeAttribute.Value = cellValueType;
                cellNode.Attributes.Append(cellValueTypeAttribute);
            }

            if (!String.IsNullOrEmpty(cellValue))
            {
                XmlAttribute cellValueAttribute = document.CreateAttribute(String.Concat("office:", cellValueName), officeUri);
                cellValueAttribute.Value = cellValue;
                cellNode.Attributes.Append(cellValueAttribute);
            }

            if (!String.IsNullOrEmpty(cellText))
            {
                XmlNode cellValueNode = document.CreateElement("text:p", textUri);
                cellValueNode.InnerText = cellText;
                cellNode.AppendChild(cellValueNode);
            }
        }
    }
}
