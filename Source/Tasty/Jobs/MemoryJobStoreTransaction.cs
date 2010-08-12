//-----------------------------------------------------------------------
// <copyright file="MemoryJobStoreTransaction.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Implements <see cref="IJobStoreTransaction"/> for the <see cref="MemoryJobStore"/>.
    /// </summary>
    public class MemoryJobStoreTransaction : IJobStoreTransaction
    {
        private MemoryJobStore jobStore;
        private List<JobRecord> saving = new List<JobRecord>();
        private List<int> deleting = new List<int>();

        /// <summary>
        /// Initializes a new instance of the MemoryJobStoreTransaction class.
        /// </summary>
        /// <param name="jobStore">The job store to initialize this instance for.</param>
        public MemoryJobStoreTransaction(MemoryJobStore jobStore)
        {
            if (jobStore == null)
            {
                throw new ArgumentNullException("jobStore", "jobStore cannot be null.");
            }

            this.jobStore = jobStore;
        }

        public void AddForSave(JobRecord record)
        {
            this.saving.Add(record);
        }

        public void AddForDelete(int jobId)
        {
            this.deleting.Add(jobId);
        }

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        public void Commit()
        {
            foreach (var record in this.saving)
            {
                this.jobStore.SaveJob(record);
            }

            foreach (var id in this.deleting)
            {
                this.jobStore.DeleteJob(id);
            }

            this.saving.Clear();
            this.deleting.Clear();
        }

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        public void Rollback()
        {
            this.saving.Clear();
            this.deleting.Clear();
        }
    }
}
