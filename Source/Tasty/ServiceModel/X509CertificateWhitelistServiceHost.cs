//-----------------------------------------------------------------------
// <copyright file="X509CertificateWhiteListServiceHost.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
//     Adapted from code by Davide Icardi, Copyright (c) Davide Icardi 2007.
//     The original can be found at http://www.codeproject.com/KB/WCF/wcfcertificates.aspx
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.ServiceModel
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using Tasty.Configuration;

    /// <summary>
    /// Extends <see cref="ServiceHost"/> to apply the custom certificate validation configuration defined
    /// in the current application's <tasty><serviceModel/></tasty> configuration.
    /// </summary>
    public class X509CertificateWhiteListServiceHost : ServiceHost
    {
        /// <summary>
        /// Initializes a new instance of the X509CertificateWhiteListServiceHost class.
        /// </summary>
        /// <param name="serviceType">The type of the service to host.</param>
        /// <param name="baseAddresses">The service's base address collection.</param>
        public X509CertificateWhiteListServiceHost(Type serviceType, Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
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
                if (element.Enabled)
                {
                    Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.Custom;
                    Credentials.ServiceCertificate.Certificate = element.LoadCertificate();

                    X509CertificateWhiteListValidator validator = new X509CertificateWhiteListValidator();

                    foreach (ClientCertificateElement clientElement in element.ClientCertificates)
                    {
                        validator.WhiteList.Add(clientElement.LoadCertificate());
                    }

                    Credentials.ClientCertificate.Authentication.CustomCertificateValidator = validator;
                }
            }
            else
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.InvariantCulture,
                        "You must configure a service with the name \"{0}\" under <tasty><serviceModel><services/></serviceModel></tasty>.",
                        Description.Name));
            }
        }
    }
}
