//-----------------------------------------------------------------------
// <copyright file="UrlTokenStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web
{
    using System;
    using Tasty.Configuration;

    /// <summary>
    /// Provides global persistence functionality and a way to access the <see cref="IUrlTokenStore"/>
    /// currently in use.
    /// </summary>
    public static class UrlTokenStore
    {
        private static readonly object currentLocker = new object();
        private static IUrlTokenStore current;

        /// <summary>
        /// Gets or sets the current <see cref="IUrlTokenStore"/> implementation in use.
        /// The setter on this property is primarily meant for testing purposes.
        /// </summary>
        public static IUrlTokenStore Current
        {
            get
            {
                lock (currentLocker)
                {
                    if (current == null)
                    {
                        current = (IUrlTokenStore)Activator.CreateInstance(Type.GetType(TastySettings.Section.UrlTokens.UrlTokenStoreType));
                    }

                    return current;
                }
            }

            set
            {
                lock (currentLocker)
                {
                    current = value;
                }
            }
        }
    }
}
