//-----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Tasty Codes">
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
    using Tasty.Configuration;

    /// <summary>
    /// Extensions and helpers for the <see cref="Tasty.ServiceModel"/> namespace.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Configures the given service client with an <see cref="X509Certificate2"/> based on the current
        /// application's <tasty><serviceModel/></tasty> configuration.
        /// </summary>
        /// <typeparam name="TChannel">The concrete channel type of the client being configured.</typeparam>
        /// <param name="client">The client to configure.</param>
        public static void ConfigureForX509WhiteList<TChannel>(this ClientBase<TChannel> client) where TChannel : class
        {
            EndpointElement element = TastySettings.Section.ServiceModel.Endpoints[client.Endpoint.Contract.ConfigurationName];

            if (element != null)
            {
                client.ClientCredentials.ClientCertificate.Certificate = element.LoadCertificate();
            }
            else
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.InvariantCulture,
                        "You must configure a client certificate for the service contract \"{0}\" under <tasty><serviceModel><endpoints/></serviceModel></tasty>.",
                        client.Endpoint.Contract.ConfigurationName));
            }
        }

        /// <summary>
        /// Loads the certificate for the given <see cref="ClientCertificateElement"/>.
        /// </summary>
        /// <param name="element">The <see cref="ClientCertificateElement"/> to load the certificate for.</param>
        /// <returns>The loaded certificate.</returns>
        internal static X509Certificate2 LoadCertificate(this ClientCertificateElement element)
        {
            if (!String.IsNullOrEmpty(element.ResourceType) && !String.IsNullOrEmpty(element.ResourceName))
            {
                using (X509CertificateLoader loader = new X509CertificateLoader(Type.GetType(element.ResourceType), element.ResourceName, element.Password))
                {
                    return loader.LoadCertificate();
                }
            }
            else if (!String.IsNullOrEmpty(element.Path))
            {
                using (X509CertificateLoader loader = new X509CertificateLoader(element.Path, element.Password))
                {
                    return loader.LoadCertificate();
                }
            }
            else
            {
                throw new ConfigurationErrorsException(
                    "Both resourceType and resourceName attributes must be set, or the path attribute must be set.",
                    element.ElementInformation.Source,
                    element.ElementInformation.LineNumber);
            }
        }

        /// <summary>
        /// Loads the certificate for the given <see cref="EndpointElement"/>.
        /// </summary>
        /// <param name="element">The <see cref="EndpointElement"/> to load the certificate for.</param>
        /// <returns>The loaded certificate.</returns>
        internal static X509Certificate2 LoadCertificate(this EndpointElement element)
        {
            if (!String.IsNullOrEmpty(element.ClientCertificateResourceType) && !String.IsNullOrEmpty(element.ClientCertificateResourceName))
            {
                using (X509CertificateLoader loader = new X509CertificateLoader(Type.GetType(element.ClientCertificateResourceType), element.ClientCertificateResourceName, element.ClientCertificatePassword))
                {
                    return loader.LoadCertificate();
                }
            }
            else if (!String.IsNullOrEmpty(element.ClientCertificatePath))
            {
                using (X509CertificateLoader loader = new X509CertificateLoader(element.ClientCertificatePath, element.ClientCertificatePassword))
                {
                    return loader.LoadCertificate();
                }
            }
            else
            {
                throw new ConfigurationErrorsException(
                    "Both clientCertificateResourceType and clientCertificateResourceName attributes must be set, or the clientCertificatePath attribute must be set.",
                    element.ElementInformation.Source,
                    element.ElementInformation.LineNumber);
            }
        }

        /// <summary>
        /// Loads the certificate for the given <see cref="ServiceElement"/>.
        /// </summary>
        /// <param name="element">The <see cref="ServiceElement"/> to load the certificate for.</param>
        /// <returns>The loaded certificate.</returns>
        internal static X509Certificate2 LoadCertificate(this ServiceElement element)
        {
            if (!String.IsNullOrEmpty(element.ServerCertificateResourceType) && !String.IsNullOrEmpty(element.ServerCertificateResourceName))
            {
                using (X509CertificateLoader loader = new X509CertificateLoader(Type.GetType(element.ServerCertificateResourceType), element.ServerCertificateResourceName, element.ServerCertificatePassword))
                {
                    return loader.LoadCertificate();
                }
            }
            else if (!String.IsNullOrEmpty(element.ServerCertificatePath))
            {
                using (X509CertificateLoader loader = new X509CertificateLoader(element.ServerCertificatePath, element.ServerCertificatePassword))
                {
                    return loader.LoadCertificate();
                }
            }
            else
            {
                throw new ConfigurationErrorsException(
                    "Both serverCertificateResourceType and serverCertificateResourceName attributes must be set, or the serverCertificatePath attribute must be set.",
                    element.ElementInformation.Source,
                    element.ElementInformation.LineNumber);
            }
        }
    }
}
