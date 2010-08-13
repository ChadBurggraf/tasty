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
        private static List<JobRecord> committed = new List<JobRecord>();

        /// <summary>
        /// Deletes a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to delete.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        public override void DeleteJob(int id, IJobStoreTransaction transaction)
        {
            lock (committed)
            {
                if (transaction != null)
                {
                    transaction.AddForDelete(id);
                }
                else
                {
                    committed.RemoveAll(r => r.Id.Value == id);
                }
            }
        }

        /// <summary>
        /// Gets a job by ID.
        /// </summary>
        /// <param name="id">The ID of the job to get.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        /// <returns>The job with the given ID.</returns>
        public override JobRecord GetJob(int id, IJobStoreTransaction transaction)
        {
            lock (committed)
            {
                return (from r in committed
                        where r.Id.Value == id
                        select new JobRecord(r)).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a collection of jobs that match the given collection of IDs.
        /// </summary>
        /// <param name="ids">The IDs of the jobs to get.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        /// <returns>A collection of jobs.</returns>
        public override IEnumerable<JobRecord> GetJobs(IEnumerable<int> ids, IJobStoreTransaction transaction)
        {
            lock (committed)
            {
                if (ids != null && ids.Count() > 0)
                {
                    return (from r in committed
                            join i in ids on r.Id.Value equals i
                            orderby r.QueueDate
                            select new JobRecord(r)).ToArray();
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
        /// <param name="transaction">The transaction to execute the command in.</param>
        /// <returns>A collection of jobs.</returns>
        public override IEnumerable<JobRecord> GetJobs(JobStatus status, int count, IJobStoreTransaction transaction)
        {
            lock (committed)
            {
                var query = from r in committed
                            where r.Status == status
                            orderby r.QueueDate
                            select new JobRecord(r);

                if (count > 0)
                {
                    return query.Take(count).ToArray();
                }

                return query.ToArray();
            }
        }

        /// <summary>
        /// Gets a collection of the most recently scheduled persisted job for each
        /// scheduled job in the given collection.
        /// </summary>
        /// <param name="scheduleNames">A collection of schedule names to get the latest persisted jobs for.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        /// <returns>A collection of recently scheduled jobs.</returns>
        public override IEnumerable<JobRecord> GetLatestScheduledJobs(IEnumerable<string> scheduleNames, IJobStoreTransaction transaction)
        {
            lock (committed)
            {
                string[] names = scheduleNames != null ? scheduleNames.ToArray() : new string[0];

                if (names.Length > 0)
                {
                    return (from r in committed
                            group r by new { r.JobType, r.ScheduleName } into g
                            where names.Contains(g.Key.ScheduleName, StringComparer.OrdinalIgnoreCase)
                            select new JobRecord(g.OrderByDescending(gr => gr.QueueDate).First())).ToArray();
                }

                return new JobRecord[0];
            }
        }

        /// <summary>
        /// Saves the given job record, either creating it or updating it.
        /// </summary>
        /// <param name="record">The job to save.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        public override void SaveJob(JobRecord record, IJobStoreTransaction transaction)
        {
            lock (committed)
            {
                if (record.Id == null)
                {
                    // Not perfect, but probably good enough.
                    record.Id = Math.Abs(Guid.NewGuid().GetHashCode());
                }

                if (transaction != null)
                {
                    transaction.AddForSave(record);
                }
                else
                {
                    committed.RemoveAll(r => r.Id.Value == record.Id.Value);
                    committed.Add(record);
                }
            }
        }

        /// <summary>
        /// Starts a transaction.
        /// </summary>
        public override IJobStoreTransaction StartTransaction()
        {
            return new MemoryJobStoreTransaction(this);
        }
    }
}
