//-----------------------------------------------------------------------
// <copyright file="JobStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Tasty.Configuration;

    /// <summary>
    /// Provides conviencence methods and a way to access the <see cref="IJobStore"/> 
    /// currently in use.
    /// </summary>
    public static class JobStore
    {
        private static readonly object locker = new object();
        private static IJobStore current;

        /// <summary>
        /// Gets or sets the current <see cref="IJobStore"/> implementation in use.
        /// The setter on this property is primarily meant for testing purposes.
        /// </summary>
        /// <remarks>
        /// It is not recommended to set this property during runtime. You should instead
        /// set it during static initialization if you would rather not infer it from
        /// the configuration. Setting it later could cause persistence errors if any
        /// currently-executing jobs try to persist their update data to the new store.
        /// </remarks>
        public static IJobStore Current
        {
            get
            {
                lock (locker)
                {
                    if (current == null)
                    {
                        current = (IJobStore)Activator.CreateInstance(Type.GetType(TastySettings.Section.Jobs.Store.JobStoreType));
                    }

                    return current;
                }
            }

            set
            {
                lock (locker)
                {
                    current = value;
                }
            }
        }
    }
}
