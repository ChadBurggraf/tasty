//-----------------------------------------------------------------------
// <copyright file="X509CertificateLoader.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.ServiceModel
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.ServiceModel;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Provides a simple service facade for loading <see cref="X509Certificate2"/>s from a stream,
    /// the filesystem or an embedded resource file.
    /// </summary>
    public class X509CertificateLoader : IDisposable
    {
        #region Private Fields

        private bool disposed;
        private Stream stream;
        private string password;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the X509CertificateLoader class.
        /// </summary>
        /// <param name="path">The path to the certificate file to load.</param>
        public X509CertificateLoader(string path)
            : this(path, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the X509CertificateLoader class.
        /// </summary>
        /// <param name="path">The path to the certificate file to load.</param>
        /// <param name="password">The certificate password, or null or empty if no password is required.</param>
        public X509CertificateLoader(string path, string password)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path", "path must contain a value.");
            }

            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
            }

            this.stream = File.OpenRead(path);
            this.password = !String.IsNullOrEmpty(password) ? password : null;
        }

        /// <summary>
        /// Initializes a new instance of the X509CertificateLoader class.
        /// </summary>
        /// <param name="type">The type identifying the assembly the embedded certificate file is in.</param>
        /// <param name="resourceName">The fully qualified name of certificate's embedded resource file.</param>
        public X509CertificateLoader(Type type, string resourceName)
            : this(type, resourceName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the X509CertificateLoader class.
        /// </summary>
        /// <param name="type">The type identifying the assembly the embedded certificate file is in.</param>
        /// <param name="resourceName">The fully qualified name of certificate's embedded resource file.</param>
        /// <param name="password">The certificate password, or null or empty if no password is required.</param>
        public X509CertificateLoader(Type type, string resourceName, string password)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type", "type cannot be null.");
            }

            if (String.IsNullOrEmpty(resourceName))
            {
                throw new ArgumentNullException("resourceName", "resourceName must contain a value.");
            }

            this.stream = type.Assembly.GetManifestResourceStream(resourceName);
            this.password = !String.IsNullOrEmpty(password) ? password : null;
        }

        /// <summary>
        /// Initializes a new instance of the X509CertificateLoader class.
        /// </summary>
        /// <param name="stream">The stream to load the certificate from.</param>
        public X509CertificateLoader(Stream stream)
            : this(stream, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the X509CertificateLoader class.
        /// </summary>
        /// <param name="stream">The stream to load the certificate from.</param>
        /// <param name="password">The certificate password, or null or empty if no password is required.</param>
        public X509CertificateLoader(Stream stream, string password)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "stream cannot be null.");
            }

            this.stream = stream;
            this.password = !String.IsNullOrEmpty(password) ? password : null;
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Loads an <see cref="X509Certificate2"/> based on this instance's construction configuration.
        /// </summary>
        /// <returns>An <see cref="X509Certificate2"/>.</returns>
        public X509Certificate2 LoadCertificate()
        {
            byte[] rawData = new byte[this.stream.Length];
            this.stream.Read(rawData, 0, rawData.Length);

            return new X509Certificate2(rawData, this.password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        /// <param name="disposing">A value indicating whether to cleanup managed resources.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.stream != null)
                    {
                        this.stream.Dispose();
                        this.stream = null;
                    }
                }

                this.disposed = true;
            }
        }

        #endregion
    }
}
