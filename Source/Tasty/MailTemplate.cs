//-----------------------------------------------------------------------
// <copyright file="MailTemplate.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Xsl;

    /// <summary>
    /// Represents an XSLT email template and its transformation.
    /// </summary>
    public class MailTemplate : IDisposable
    {
        private bool disposed, streamDisposable;
        private Stream templateStream;

        /// <summary>
        /// Initializes a new instance of the MailTemplate class.
        /// </summary>
        /// <param name="templateStream">The stream of template data to initialize this instance with.</param>
        public MailTemplate(Stream templateStream)
        {
            if (templateStream == null)
            {
                throw new ArgumentNullException("templateStream", "templateStream cannot be null.");
            }

            this.templateStream = templateStream;
        }

        /// <summary>
        /// Initializes a new instance of the MailTemplate class.
        /// </summary>
        /// <param name="templatePath">The path to the XSLT template file to use.</param>
        public MailTemplate(string templatePath)
        {
            if (String.IsNullOrEmpty(templatePath))
            {
                throw new ArgumentNullException("templatePath", "templatePath must contain a value.");
            }

            if (!File.Exists(templatePath))
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, @"The path ""{0}"" does not exist.", templatePath), "templatePath");
            }

            this.templateStream = File.OpenRead(templatePath);
            this.streamDisposable = true;
        }

        /// <summary>
        /// Finalizes an instance of the MailTemplate class.
        /// </summary>
        ~MailTemplate()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Transforms the given model using this instance's template.
        /// </summary>
        /// <param name="model">The model to transform.</param>
        /// <returns>A string of XML representing the transformed model.</returns>
        public string Transform(MailModel model)
        {
            StringBuilder sb = new StringBuilder();

            using (StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                using (XmlWriter xw = new XmlTextWriter(sw))
                {
                    this.Transform(model, xw);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Transforms the given model using this instance's template.
        /// </summary>
        /// <param name="model">The model to transform.</param>
        /// <param name="writer">The <see cref="XmlWriter"/> to write the results of the transformation to.</param>
        public void Transform(MailModel model, XmlWriter writer)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model", "model cannot be null.");
            }

            if (writer == null)
            {
                throw new ArgumentNullException("writer", "writer cannot be null.");
            }

            XmlDocument stylesheet = new XmlDocument();
            stylesheet.Load(this.templateStream);

            XslCompiledTransform transform = new XslCompiledTransform();
            transform.Load(stylesheet);

            transform.Transform(model.ToXml(), writer);
        }

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        /// <param name="disposing">A value indicating whether to actively dispose of managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.templateStream != null && this.streamDisposable)
                    {
                        this.templateStream.Dispose();
                        this.templateStream = null;
                    }
                }

                this.disposed = true;
            }
        }
    }
}
