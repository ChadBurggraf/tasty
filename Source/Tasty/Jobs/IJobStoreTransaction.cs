//-----------------------------------------------------------------------
// <copyright file="IJobStoreTransaction.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;

    /// <summary>
    /// Identifies the interface for <see cref="IJobStore"/> transactions.
    /// </summary>
    public interface IJobStoreTransaction : IDisposable
    {
        /// <summary>
        /// Adds the given job ID for deletion to the transaction.
        /// </summary>
        /// <param name="jobId">The ID of the job to delete.</param>
        void AddForDelete(int jobId);

        /// <summary>
        /// Adds the given record for saving to the transaction.
        /// </summary>
        /// <param name="record">The record to save.</param>
        void AddForSave(JobRecord record);

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        void Commit();

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        void Rollback();
    }
}
