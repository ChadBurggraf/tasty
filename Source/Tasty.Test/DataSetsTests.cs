using System;
using System.Data;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tasty.Test
{
    [TestClass]
    public class DataSetsTests
    {
        [TestMethod]
        public void DataSets_WriteToOdsFile()
        {
            DataSet ds = new DataSet("Chad's Data Set");

            DataTable dt1 = ds.Tables.Add("Data Table 01");
            dt1.Columns.Add("Name", typeof(string));
            dt1.Columns.Add("Number", typeof(int));
            dt1.Columns.Add("Sign", typeof(string));

            dt1.Rows.Add("Chad B", 42, "Gemini");
            dt1.Rows.Add("Sparky", -11, "Huh, god knows");
            dt1.Rows.Add("Mr. Garrison", 1024, "Scorpio");

            DataTable dt2 = ds.Tables.Add("Data Table 02");
            dt2.Columns.Add("Date", typeof(DateTime));

            dt2.Rows.Add(DateTime.Now);
            dt2.Rows.Add(new DateTime(1970, 1, 1));
            dt2.Rows.Add(new DateTime(2012, 1, 1));
            dt2.Rows.Add(new DateTime(1982, 5, 28));

            if (File.Exists("dataset.ods"))
            {
                File.Delete("dataset.ods");
            }

            ds.WriteToOdsFile("dataset.ods");
            Assert.IsTrue(File.Exists("dataset.ods"));
        }
    }
}
