//-----------------------------------------------------------------------
// <copyright file="ServiceModelTests.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tasty.ServiceModel;

    /// <summary>
    /// Service model tests.
    /// </summary>
    [TestClass]
    public class ServiceModelTests
    {
        /// <summary>
        /// Load file system certificates tests.
        /// </summary>
        [TestMethod]
        public void ServiceModelLoadFileSystemCertificates()
        {
            using (X509CertificateLoader loader = new X509CertificateLoader(Path.GetFullPath("TastyClient.cer")))
            {
                Assert.IsNotNull(loader.LoadCertificate());
            }

            using (X509CertificateLoader loader = new X509CertificateLoader(Path.GetFullPath("TastyClient.pfx"), "tastyclient"))
            {
                Assert.IsNotNull(loader.LoadCertificate());
            }

            using (X509CertificateLoader loader = new X509CertificateLoader(Path.GetFullPath("TastyServer.pfx"), "tastyserver"))
            {
                Assert.IsNotNull(loader.LoadCertificate());
            }
        }

        /// <summary>
        /// Load embedded resource certificates tests.
        /// </summary>
        [TestMethod]
        public void ServiceModelLoadResourceCertificates()
        {
            using (X509CertificateLoader loader = new X509CertificateLoader(GetType(), "Tasty.Test.TastyClient.cer"))
            {
                Assert.IsNotNull(loader.LoadCertificate());
            }

            using (X509CertificateLoader loader = new X509CertificateLoader(GetType(), "Tasty.Test.TastyClient.pfx", "tastyclient"))
            {
                Assert.IsNotNull(loader.LoadCertificate());
            }

            using (X509CertificateLoader loader = new X509CertificateLoader(GetType(), "Tasty.Test.TastyServer.pfx", "tastyserver"))
            {
                Assert.IsNotNull(loader.LoadCertificate());
            }
        }
    }
}
