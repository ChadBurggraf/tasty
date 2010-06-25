//-----------------------------------------------------------------------
// <copyright file="X509CertificateWhitelistServiceHostFactory.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.ServiceModel
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Activation;

    /// <summary>
    /// Extends <see cref="ServiceHostFactory"/> to create <see cref="X509CertificateWhitelistServiceHost"/> instances.
    /// </summary>
    public class X509CertificateWhitelistServiceHostFactory : ServiceHostFactory
    {
        /// <summary>
        /// Creates a <see cref="ServiceHost"/> for the specified type of service.
        /// </summary>
        /// <param name="serviceType">The type of service to create the <see cref="ServiceHost"/> for.</param>
        /// <param name="baseAddresses">The collection of base addresses to create the <see cref="ServiceHost"/> with.</param>
        /// <returns>The created <see cref="ServiceHost"/></returns>
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return new X509CertificateWhitelistServiceHost(serviceType, baseAddresses);
        }
    }
}
