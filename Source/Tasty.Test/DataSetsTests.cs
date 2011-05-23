//-----------------------------------------------------------------------
// <copyright file="DataSetsTests.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// DataSet tests.
    /// </summary>
    [TestClass]
    public class DataSetsTests
    {
        /// <summary>
        /// Write to ODS file tests.
        /// </summary>
        [TestMethod]
        public void DataSetsWriteToOdsFile()
        {
            if (File.Exists("dataset.ods"))
            {
                File.Delete("dataset.ods");
            }

            CreateTestDataSet().WriteToOdsFile("dataset.ods");
            Assert.IsTrue(File.Exists("dataset.ods"));
        }

        /// <summary>
        /// Write to XLSX file tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Acronym.")]
        public void DataSetsWriteToXlsxFile()
        {
            if (File.Exists("dataset.xlsx"))
            {
                File.Delete("dataset.xlsx");
            }

            CreateTestDataSet().WriteToXlsxFile("dataset.xlsx");
            Assert.IsTrue(File.Exists("dataset.xlsx"));
        }

        /// <summary>
        /// Creates a test DataSet.
        /// </summary>
        /// <returns>A test DataSet.</returns>
        private static DataSet CreateTestDataSet()
        {
            DataSet ds = new DataSet("Chad's Data Set") { Locale = CultureInfo.InvariantCulture };

            DataTable dt1 = ds.Tables.Add("Data Table 01");
            dt1.Columns.Add("Name", typeof(string));
            dt1.Columns.Add("Number", typeof(int));
            dt1.Columns.Add("Sign", typeof(string));

            dt1.Rows.Add("Chad B", 42, "Gemini");
            dt1.Rows.Add("Sparky", -11, "Huh, god knows");
            dt1.Rows.Add("Mr. Garrison", 1024, "Scorpio");

            DataTable dt2 = ds.Tables.Add("Data Table 02");
            dt2.Columns.Add("Date", typeof(DateTime));
            dt2.Columns.Add("Float", typeof(float));
            dt2.Columns.Add("Time", typeof(DateTime));

            dt2.Rows.Add(DateTime.Now, 42.67, DateTime.MinValue);
            dt2.Rows.Add(new DateTime(1970, 1, 1), -112, DateTime.MinValue.AddHours(16.5));
            dt2.Rows.Add(new DateTime(2012, 1, 1), 0, DateTime.MinValue.AddHours(10.25));
            dt2.Rows.Add(new DateTime(1982, 5, 28), 356.9999999, DateTime.MinValue.AddHours(23));

            return ds;
        }
    }
}
