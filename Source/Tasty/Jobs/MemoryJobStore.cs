//-----------------------------------------------------------------------
// <copyright file="MemoryJobStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tasty.Configuration;

    /// <summary>
    /// Implements <see cref="IJobStore"/> as a transient, in-memory job store.
    /// </summary>
    public class MemoryJobStore : JobStore
    {
        private Dictionary<int, JobRecord> committed = new Dictionary<int, JobRecord>();
        private readonly object locker = new object();

        /// <summary>
        /// Deletes a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to delete.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        public override void DeleteJob(int id, IJobStoreTransaction transaction)
        {
            lock (this.locker)
            {
                if (transaction != null)
                {
                    transaction.Add
                }
                if (this.isInTransaction)
                {
                    this.deleting.Add(id);
                }
                else
                {
                    this.committed.Remove(id);
                }
            }
        }

        /// <summary>
        /// Gets a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to get.</param>
        /// <returns>The job with the given ID.</returns>
        public override JobRecord GetJob(int id)
        {
            lock (this.locker)
            {
                if (this.committed.ContainsKey(id))
                {
                    return this.committed[id];
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a collection of jobs that match the given collection of IDs.
        /// </summary>
        /// <param name="ids">The IDs of the jobs to get.</param>
        /// <returns>A collection of jobs.</returns>
        public override IEnumerable<JobRecord> GetJobs(IEnumerable<int> ids)
        {
            lock (this.locker)
            {
                if (ids != null)
                {
                    return (from r in this.committed.Values
                            where ids.Contains(r.Id.Value)
                            orderby r.QueueDate descending
                            select r).ToArray();
                }

                return new JobRecord[0];
            }
        }

        /// <summary>
        /// Gets a collection of jobs with the given status, returning
        /// at most the number of jobs identified by <paramref name="count"/>.
        /// </summary>
        /// <param name="status">The status of the jobs to get.</param>
        /// <param name="count">The maximum number of jobs to get.</param>
        /// <returns>A collection of jobs.</returns>
        public override IEnumerable<JobRecord> GetJobs(JobStatus status, int count)
        {
            lock (this.locker)
            {
                var query = from r in this.committed.Values
                            where r.Status == status
                            orderby r.QueueDate descending
                            select r;

                if (count > 0)
                {
                    return query.Take(count).ToArray();
                }

                return query.ToArray();
            }
        }

        /// <summary>
        /// Gets a collection of the most recently scheduled persisted job for each
        /// scheduled job in the configuration.
        /// </summary>
        /// <returns>A collection of recently scheduled jobs.</returns>
        public override IEnumerable<JobRecord> GetLatestScheduledJobs()
        {
            lock (this.locker)
            {
                return (from r in this.committed.Values
                        group r by r.ScheduleName into g
                        where TastySettings.Section.Jobs.Schedules.Select(s => s.Name).Contains(g.Key)
                        select g.OrderByDescending(gr => gr.QueueDate).First()).ToArray();
            }
        }

        /// <summary>
        /// Initializes the job store with the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration to initialize the job store with.</param>
        public override void Initialize(TastySettings configuration)
        {
        }

        /// <summary>
        /// Rolls back the currently in-progress transaction.
        /// </summary>
        public override void RollbackTransaction()
        {
            lock (this.locker)
            {
                if (this.isInTransaction)
                {
                    this.saving.Clear();
                    this.deleting.Clear();
                    this.isInTransaction = false;
                }
            }
        }

        /// <summary>
        /// Saves the given job record, either creating it or updating it.
        /// </summary>
        /// <param name="record">The job to save.</param>
        public override void SaveJob(JobRecord record)
        {
            lock (this.locker)
            {
                if (this.isInTransaction)
                {
                    this.saving.Add(record);
                }
                else
                {
                    if (record.Id == null)
                    {
                        record.Id = GetNewId();
                    }

                    this.committed[record.Id.Value] = record;
                }
            }
        }

        /// <summary>
        /// Starts a transaction.
        /// </summary>
        public override void StartTransaction()
        {
            lock (this.locker)
            {
                this.isInTransaction = true;
            }
        }

        /// <summary>
        /// Gets a new, unique ID.
        /// </summary>
        /// <returns>A new ID.</returns>
        private static int GetNewId()
        {
            return Math.Abs(Guid.NewGuid().GetHashCode());
        }
    }
}
