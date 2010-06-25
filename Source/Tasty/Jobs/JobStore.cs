

namespace Tasty.Jobs
{
    using System;
    using Tasty.Configuration;

    /// <summary>
    /// Provides a way to access the currently configured <see cref="IJobStore"/> instance.
    /// </summary>
    public static class JobStore
    {
        private static readonly object locker = new object();
        private static IJobStore current;

        /// <summary>
        /// Gets the currently configured <see cref="IJobStore"/> instance.
        /// </summary>
        public static IJobStore Current
        {
            get
            {
                lock (locker)
                {
                    if (current == null)
                    {
                        Type type = Type.GetType(TastySettings.Section.Jobs.Store.JobStoreType);
                        current = (IJobStore)Activator.CreateInstance(type);
                    }

                    return current;
                }
            }
        }

        /// <summary>
        /// Sets the current <see cref="IJobStore"/> instance. This method is intended primarily for testing purposes.
        /// </summary>
        /// <remarks>
        /// It is not recommended to set this property during runtime. You should instead
        /// set it during static initialization if you would rather not infer it from
        /// the configuration. Setting it later could cause persistence errors if any
        /// currently-executing jobs try to persist their update data to the new store.
        /// </remarks>
        /// <param name="jobStore">The new <see cref="IJobStore"/> instance to set.</param>
        public static void SetCurrent(IJobStore jobStore)
        {
            lock (locker)
            {
                current = jobStore;
            }
        }
    }
}
