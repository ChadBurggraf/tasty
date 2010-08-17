

namespace Tasty.Test
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tasty.ServiceModel;

    [TestClass]
    public class ServiceModelTests
    {
        [TestMethod]
        public void ServiceModel_LoadFilesystemCertificates()
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

        [TestMethod]
        public void ServiceModel_LoadResourceCertificates()
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
