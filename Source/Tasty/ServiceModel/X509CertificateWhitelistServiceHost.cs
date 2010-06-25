//-----------------------------------------------------------------------
// <copyright file="X509CertificateWhitelistServiceHost.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
//     Adapted from code by Davide Icardi, Copyright (c) Davide Icardi 2007.
//     The original can be found at http://www.codeproject.com/KB/WCF/wcfcertificates.aspx
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.ServiceModel
{
    using System;
    using System.IO;
    using System.ServiceModel;
    using System.Configuration;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Security;
    using Tasty.Configuration;

    /// <summary>
    /// Extends <see cref="ServiceHost"/> to apply the custom certificate validation configuration defined
    /// in the current application's <tasty><serviceModel/></tasty> configuration.
    /// </summary>
    public class X509CertificateWhitelistServiceHost : ServiceHost
    {
        private string serverCertificatePassword;

        /// <summary>
        /// Initializes a new instance of the X509CertificateWhitelistServiceHost class.
        /// </summary>
        /// <param name="serviceType">The type of the service to host.</param>
        /// <param name="baseAddresses">The service's base address collection.</param>
        public X509CertificateWhitelistServiceHost(Type serviceType, Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
        }

        /// <summary>
        /// Initializes a new instance of the X509CertificateWhitelistServiceHost class.
        /// </summary>
        /// <param name="serviceType">The type of the service to host.</param>
        /// <param name="baseAddresses">The service's base address collection.</param>
        /// <param name="serverCertificatePassword">The password to use when loading the server certificate, if required.</param>
        public X509CertificateWhitelistServiceHost(Type serviceType, Uri[] baseAddresses, string serverCertificatePassword)
            : this(serviceType, baseAddresses)
        {
            this.serverCertificatePassword = serverCertificatePassword ?? String.Empty;
        }

        /// <summary>
        /// Loads the service description information from the configuration and applies it to the runtime being constructed.
        /// </summary>
        protected override void ApplyConfiguration()
        {
            base.ApplyConfiguration();

            ServiceElement element = TastySettings.Section.ServiceModel.Services[Description.Name];

            if (element != null)
            {
                string path = element.ServerCertificate;

                if (!Path.IsPathRooted(path))
                {
                    path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
                }

                Credentials.ServiceCertificate.Certificate = new X509Certificate2(path, this.serverCertificatePassword);
                Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.Custom;

                X509CertificateWhitelistValidator validator = new X509CertificateWhitelistValidator();

                foreach (ClientCertificateElement clientElement in element.ClientCertificates)
                {
                    path = clientElement.ClientCertificate;

                    if (!Path.IsPathRooted(path))
                    {
                        path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
                    }

                    validator.Whitelist.Add(new X509Certificate2(path));
                }

                Credentials.ClientCertificate.Authentication.CustomCertificateValidator = validator;
            }
        }
    }
}
