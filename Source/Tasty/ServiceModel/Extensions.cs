//-----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
//     Adapted from code by Davide Icardi, Copyright (c) Davide Icardi 2007.
//     The original can be found at http://www.codeproject.com/KB/WCF/wcfcertificates.aspx
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.ServiceModel
{
    using System;
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
        public static void ConfigureForX509Whitelist<TChannel>(this ClientBase<TChannel> client)
        {
            EndpointElement element = TastySettings.Section.ServiceModel.Endpoints[client.Endpoint.Contract.ConfigurationName];

            if (element != null)
            {
                string path = element.ClientCertificate;

                if (!Path.IsPathRooted(path))
                {
                    path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
                }

                client.ClientCredentials.ClientCertificate.Certificate = new X509Certificate2(path);
            }
        }
    }
}
