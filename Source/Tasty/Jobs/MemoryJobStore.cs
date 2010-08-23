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
        private int hashCode;

        /// <summary>
        /// Initializes a new instance of the MemoryJobStore class.
        /// </summary>
        public MemoryJobStore()
        {
            this.hashCode = Guid.NewGuid().GetHashCode();
        }

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
        /// Gets the hash code for this instance.
        /// </summary>
        /// <returns>This instance's hash code.</returns>
        public override int GetHashCode()
        {
            return this.hashCode;
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
        /// Gets the number of jobs in the store that match the given filter.
        /// </summary>
        /// <param name="likeName">A string representing a full or partial job name to filter on.</param>
        /// <param name="withStatus">A <see cref="JobStatus"/> to filter on, or null if not applicable.</param>
        /// <param name="inSchedule">A schedule name to filter on, if applicable.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        /// <returns>The number of jobs that match the given filter.</returns>
        public override int GetJobCount(string likeName, JobStatus? withStatus, string inSchedule, IJobStoreTransaction transaction)
        {
            lock (committed)
            {
                var query = committed.AsQueryable();

                if (!String.IsNullOrEmpty(likeName))
                {
                    query = query.Where(r => likeName.Contains(r.Name, StringComparison.OrdinalIgnoreCase));
                }

                if (withStatus != null)
                {
                    query = query.Where(r => r.Status == withStatus.Value);
                }

                if (!String.IsNullOrEmpty(inSchedule))
                {
                    query = query.Where(r => inSchedule.Equals(r.ScheduleName, StringComparison.OrdinalIgnoreCase));
                }

                return query.Count();
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
        /// Gets a collection of jobs that match the given filter parameters, ordered by the given sort parameters.
        /// </summary>
        /// <param name="likeName">A string representing a full or partial job name to filter on.</param>
        /// <param name="withStatus">A <see cref="JobStatus"/> to filter on, or null if not applicable.</param>
        /// <param name="inSchedule">A schedule name to filter on, if applicable.</param>
        /// <param name="orderBy">A field to order the resultset by.</param>
        /// <param name="sortDescending">A value indicating whether to order the resultset in descending order.</param>
        /// <param name="pageNumber">The page number to get.</param>
        /// <param name="pageSize">The size of the pages to get.</param>
        /// <param name="transaction">The transaction to execute the command in.</param>
        /// <returns>A collection of jobs.</returns>
        public override IEnumerable<JobRecord> GetJobs(string likeName, JobStatus? withStatus, string inSchedule, JobRecordResultsOrderBy orderBy, bool sortDescending, int pageNumber, int pageSize, IJobStoreTransaction transaction)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentException("pageNumber must be greater than 0.", "pageNumber");
            }

            if (pageSize < 1)
            {
                throw new ArgumentException("pageSize must be greater than 0.", "pageSize");
            }

            lock (committed)
            {
                var query = committed.AsQueryable();

                if (!String.IsNullOrEmpty(likeName))
                {
                    query = query.Where(r => r.Name.Contains(likeName, StringComparison.OrdinalIgnoreCase));
                }

                if (withStatus != null)
                {
                    query = query.Where(r => r.Status == withStatus.Value);
                }

                if (!String.IsNullOrEmpty(inSchedule))
                {
                    query = query.Where(r => r.ScheduleName.Equals(inSchedule, StringComparison.OrdinalIgnoreCase));
                }

                Func<JobRecord, object> sorter;

                switch (orderBy)
                {
                    case JobRecordResultsOrderBy.FinishDate:
                        sorter = r => r.FinishDate;
                        break;
                    case JobRecordResultsOrderBy.JobType:
                        sorter = r => r.JobType;
                        break;
                    case JobRecordResultsOrderBy.Name:
                        sorter = r => r.Name;
                        break;
                    case JobRecordResultsOrderBy.QueueDate:
                        sorter = r => r.QueueDate;
                        break;
                    case JobRecordResultsOrderBy.ScheduleName:
                        sorter = r => r.ScheduleName;
                        break;
                    case JobRecordResultsOrderBy.StartDate:
                        sorter = r => r.StartDate;
                        break;
                    case JobRecordResultsOrderBy.Status:
                        sorter = r => r.Status;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                var sorted = sortDescending ? query.OrderByDescending(sorter) : query.OrderBy(sorter);
                return sorted.Skip((pageNumber - 1) * pageSize).Take(pageSize);
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
        /// <returns>A new <see cref="IJobStoreTransaction"/>.</returns>
        public override IJobStoreTransaction StartTransaction()
        {
            return new MemoryJobStoreTransaction(this);
        }
    }
}
