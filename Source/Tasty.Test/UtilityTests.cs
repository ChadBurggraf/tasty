using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tasty.Test
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void Utility_CamelCaseToLowercaseUnderscore()
        {
            Assert.AreEqual("pascal_case", "PascalCase".ToLowercaseUnderscore());
            Assert.AreEqual("camel_case", "camelCase".ToLowercaseUnderscore());
            Assert.AreEqual("'camel0_case'", "'Camel0Case'".ToLowercaseUnderscore());
        }

        [TestMethod]
        public void Utility_CopyProperties()
        {
            var source1 = new CopyPropertiesTest() { Name = Guid.NewGuid().ToString(), Number = 42, Date = DateTime.UtcNow };
            var dest1 = new CopyPropertiesTest() { Name = "some string", Number = 0, Date = DateTime.MinValue };

            source1.CopyProperties(dest1);

            Assert.AreEqual(source1.Name, dest1.Name);
            Assert.AreEqual(source1.Number, dest1.Number);
            Assert.AreEqual(source1.Date, dest1.Date);
        }

        [TestMethod]
        public void Utility_EnumDescriptions()
        {
            Assert.AreEqual("II", DescriptionEnum.Two.ToDescription());
            Assert.AreEqual(DescriptionEnum.Three, "III".EnumFromDescription<DescriptionEnum>());
        }

        [TestMethod]
        public void Utility_LowerCaseUnderScoreToCamelCase()
        {
            Assert.AreEqual("PascalCase", "pascal_case".FromLowercaseUnderscore());
            Assert.AreEqual("camelCase", "camel_case".FromLowercaseUnderscore(true));
            Assert.AreEqual("'Pascal0Case'", "'pascal0_case'".FromLowercaseUnderscore());
        }

        [TestMethod]
        public void Utility_ToPrettyTime()
        {
            TimeSpan timeSpan = new TimeSpan(3, 6, 27, 38);
            Assert.AreEqual("3d 6h", timeSpan.ToPrettyString());

            timeSpan = new TimeSpan(0, 4, 36, 59, 875);
            Assert.AreEqual("4h 36m", timeSpan.ToPrettyString());

            timeSpan = new TimeSpan(0, 0, 12, 17, 0);
            Assert.AreEqual("12m 17s", timeSpan.ToPrettyString());

            timeSpan = new TimeSpan(0, 0, 0, 2, 850);
            Assert.AreEqual("2.8s", timeSpan.ToPrettyString());
            
            timeSpan = new TimeSpan(0, 0, 0, 0, 300);
            Assert.AreEqual("0.3s", timeSpan.ToPrettyString());
        }
    }

    internal class CopyPropertiesTest
    {
        public DateTime Date { get; set; }

        public string Name { get; set; }

        public int Number { get; set; }
    }

    internal enum DescriptionEnum
    {
        [System.ComponentModel.Description("I")]
        One,

        [System.ComponentModel.Description("II")]
        Two,

        [System.ComponentModel.Description("III")]
        Three
    }
}
