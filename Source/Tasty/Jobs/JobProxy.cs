
namespace Tasty.Jobs
{
    using System;
    using System.Reflection;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides proxy methods for interacting with <see cref="IJob"/> instances
    /// in separate <see cref="AppDomain"/>s.
    /// </summary>
    public class JobProxy : MarshalByRefObject
    {
        private bool initialized;
        private Type jobType;
        private IJob job;

        /// <summary>
        /// Gets the wrapped job's name.
        /// </summary>
        public string JobName
        {
            get
            {
                EnsureInitialized();
                return this.job.Name;
            }
        }

        /// <summary>
        /// Gets the wrapped job's timeout.
        /// </summary>
        public long Timeout
        {
            get
            {
                EnsureInitialized();
                return this.job.Timeout;
            }
        }

        /// <summary>
        /// Gets the job type this instance wraps.
        /// </summary>
        public Type JobType
        {
            get
            {
                EnsureInitialized();
                return this.jobType;
            }
        }

        /// <summary>
        /// Executes the wrapped job.
        /// </summary>
        public void Execute()
        {
            this.EnsureInitialized();
            this.job.Execute();
        }

        /// <summary>
        /// Initializes this instance for a specific <see cref="IJob"/> type.
        /// </summary>
        /// <param name="assembly">The assembly to load the job from.</param>
        /// <param name="typeName">The type name of the job to load.</param>
        public void Initialize(Assembly assembly, string typeName)
        {
            if (!this.initialized)
            {
                this.jobType = assembly.GetType(typeName.TypeNameWithoutAssembly(), true);
                this.job = (IJob)Activator.CreateInstance(this.jobType);
                this.initialized = true;
            }
        }

        /// <summary>
        /// Ensures that this instance has been initialized.
        /// </summary>
        private void EnsureInitialized() 
        {
            if (!this.initialized)
            {
                throw new InvalidOperationException("This instance has not been initialized.");
            }
        }
    }
}
