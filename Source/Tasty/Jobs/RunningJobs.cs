﻿//-----------------------------------------------------------------------
// <copyright file="RunningJobs.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a collection of running jobs that can be flushed to disk.
    /// </summary>
    [DataContract]
    public sealed class RunningJobs
    {
        private List<JobRun> runs;

        /// <summary>
        /// Initializes a new instance of the RunningJobs class.
        /// </summary>
        public RunningJobs()
        {
            this.PersistencePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), GeneratePersistenceFileName(JobStore.Current));
            this.runs = new List<JobRun>(LoadFromPersisted(this.PersistencePath));
        }

        /// <summary>
        /// Initializes a new instance of the RunningJobs class.
        /// </summary>
        /// <param name="persistencePath">The persistence path to use when persisting run data.</param>
        public RunningJobs(string persistencePath)
        {
            if (String.IsNullOrEmpty(persistencePath))
            {
                throw new ArgumentNullException("persistencePath", "persistencePath must contain a value.");
            }

            if (!Path.IsPathRooted(persistencePath))
            {
                persistencePath = Path.GetFullPath(persistencePath);
            }

            if (!persistencePath.StartsWith(Path.GetDirectoryName(GetType().Assembly.Location), StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("persistencePath must point to a path inside the current application directory.", "persistencePath");
            }

            this.PersistencePath = persistencePath;
            this.runs = new List<JobRun>(LoadFromPersisted(this.PersistencePath));
        }

        /// <summary>
        /// Gets the number of job runs this instance contains.
        /// </summary>
        public int Count
        {
            get { return this.runs.Count; }
        }

        /// <summary>
        /// Gets the path used to persist the running jobs state.
        /// </summary>
        public string PersistencePath { get; private set; }

        /// <summary>
        /// Generates a stable persistence file name for the given job store.
        /// The name will be unique for the type, and remain the same as long as the type's
        /// name and assembly name (not including version number or public key) do not change.
        /// </summary>
        /// <param name="store">The job store to generate the persistence file name for.</param>
        /// <returns>The generated persistence file name.</returns>
        public static string GeneratePersistenceFileName(IJobStore store)
        {
            return String.Concat(store.GetHashCode().ToString(CultureInfo.InvariantCulture).Hash(), ".xml");
        }

        /// <summary>
        /// Gets all of the job runs this instance is maintaining.
        /// </summary>
        /// <returns>A collection of job runs.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "I want to encourange caching the results of this call into a variable.")]
        public IEnumerable<JobRun> GetAll()
        {
            lock (this.runs)
            {
                return this.runs.ToArray();
            }
        }

        /// <summary>
        /// Gets all of the job runs marked as not-running this instance is maintaining.
        /// </summary>
        /// <returns>A collection of job runs.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "I want to encourange caching the results of this call into a variable.")]
        public IEnumerable<JobRun> GetNotRunning()
        {
            lock (this.runs)
            {
                return (from j in this.runs
                        where !j.IsRunning
                        select j).ToArray();
            }
        }

        /// <summary>
        /// Gets all of the job runs marked as running this instance is maintaining.
        /// </summary>
        /// <returns>A collection of job runs.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "I want to encourange caching the results of this call into a variable.")]
        public IEnumerable<JobRun> GetRunning()
        {
            lock (this.runs)
            {
                return (from j in this.runs
                        where j.IsRunning
                        select j).ToArray();
            }
        }

        /// <summary>
        /// Adds a job run to this instance.
        /// </summary>
        /// <param name="jobRun">The job run to add.</param>
        public void Add(JobRun jobRun)
        {
            lock (this.runs)
            {
                this.runs.Add(jobRun);
            }
        }

        /// <summary>
        /// Flushes this instance's state to disk.
        /// </summary>
        public void Flush()
        {
            lock (this.runs)
            {
                var exceptionTypes = (from r in this.runs
                                      where r.ExecutionException != null
                                      select r.ExecutionException.GetType()).Distinct();

                DataContractSerializer serializer = new DataContractSerializer(typeof(JobRun[]), exceptionTypes);

                using (FileStream stream = File.Create(this.PersistencePath))
                {
                    serializer.WriteObject(stream, this.runs.ToArray());
                }
            }
        }

        /// <summary>
        /// Removes the job run with the specified ID.
        /// </summary>
        /// <param name="jobId">The ID of the job run to remove.</param>
        public void Remove(int jobId)
        {
            lock (this.runs)
            {
                this.runs.RemoveAll(r => r.JobId == jobId);
            }
        }

        /// <summary>
        /// Loads a collection of job runs from the given persistence path.
        /// </summary>
        /// <param name="persistencPath">The persistenc path to load job runs from.</param>
        /// <returns>The loaded job run collection.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "It's not worth it to enumarte all of the possible failure scenarios. We're okay with failing in general.")]
        private static IEnumerable<JobRun> LoadFromPersisted(string persistencPath)
        {
            IEnumerable<JobRun> runs;

            if (File.Exists(persistencPath))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(JobRun[]));

                try
                {
                    using (FileStream stream = File.OpenRead(persistencPath))
                    {
                        runs = (JobRun[])serializer.ReadObject(stream);
                    }
                }
                catch
                {
                    runs = new JobRun[0];
                }
            }
            else
            {
                runs = new JobRun[0];
            }

            DateTime now = DateTime.UtcNow;

            foreach (JobRun job in runs)
            {
                job.SetStateForRecovery(now);
            }

            return runs;
        }
    }
}
