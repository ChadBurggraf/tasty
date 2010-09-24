//-----------------------------------------------------------------------
// <copyright file="MailModel.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;
    using System.Xml.XPath;

    /// <summary>
    /// Represents the base class for templated email models.
    /// </summary>
    [DataContract(Namespace = MailModel.XmlNamespace)]
    public abstract class MailModel
    {
        /// <summary>
        /// Gets the XML namespace used during mail model serialization.
        /// </summary>
        public const string XmlNamespace = "http://tastycodes.com/tasty-dll/mailmodel/";

        private DateTime? today;

        /// <summary>
        /// Gets or sets the destination address of the email being modeled.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the current date.
        /// </summary>
        [DataMember(IsRequired = true)]
        public DateTime Today
        {
            get { return (DateTime)(this.today ?? (this.today = DateTime.Now)); }
            set { this.today = value; }
        }

        /// <summary>
        /// Serializes this instance to XML.
        /// </summary>
        /// <returns>The serialized XML.</returns>
        public IXPathNavigable ToXml()
        {
            DataContractSerializer serializer = new DataContractSerializer(GetType());
            StringBuilder sb = new StringBuilder();

            using (StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                using (XmlWriter xw = new XmlTextWriter(sw))
                {
                    serializer.WriteObject(xw, this);
                }
            }

            XmlDocument document = new XmlDocument();
            document.LoadXml(sb.ToString());

            return document;
        }
    }
}
