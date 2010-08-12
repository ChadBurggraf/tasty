//-----------------------------------------------------------------------
// <copyright file="SqlJobStoreTransaction.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Data.Common;
    using System.Globalization;

    /// <summary>
    /// Implements <see cref="IJobStoreTransaction"/> for <see cref="SqlJobStore"/>s.
    /// </summary>
    public class SqlJobStoreTransaction : IJobStoreTransaction
    {
        private SqlJobStore jobStore;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the SqlJobStoreTransaction class.
        /// </summary>
        /// <param name="jobStore">The <see cref="SqlJobStore"/> to create the transaction for.</param>
        /// <param name="connection">The database connection to create the transaction for.</param>
        /// <param name="transaction">The inner transaction to create the transaction for.</param>
        public SqlJobStoreTransaction(SqlJobStore jobStore, DbConnection connection, DbTransaction transaction)
        {
            if (jobStore == null)
            {
                throw new ArgumentNullException("jobStore", "jobStore cannot be null.");
            }

            if (connection == null)
            {
                throw new ArgumentNullException("connection", "connection cannot be null.");
            }

            if (transaction == null)
            {
                throw new ArgumentNullException("transaction", "transaction cannot be null.");
            }

            this.jobStore = jobStore;
            this.Connection = connection;
            this.Transaction = transaction;
        }

        /// <summary>
        /// Gets this instance's open database connection.
        /// </summary>
        public DbConnection Connection { get; private set; }

        /// <summary>
        /// Gets this instance's inner transaction.
        /// </summary>
        public DbTransaction Transaction { get; private set; }

        /// <summary>
        /// Adds the given job ID for deletion to the transaction.
        /// </summary>
        /// <param name="jobId">The ID of the job to delete.</param>
        public void AddForDelete(int jobId)
        {
            using (DbCommand command = this.jobStore.CreateDeleteCommand(this.Connection, this.Transaction, jobId))
            {
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Adds the given record for saving to the transaction.
        /// </summary>
        /// <param name="record">The record to save.</param>
        public void AddForSave(JobRecord record)
        {
            using (DbCommand command = this.jobStore.CreateSaveCommand(this.Connection, this.Transaction, record))
            {
                using (DbDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        record.Id = Convert.ToInt32(reader[0], CultureInfo.InvariantCulture);
                    }
                }
            }
        }

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        public void Commit()
        {
            this.Transaction.Commit();
        }

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        public void Rollback()
        {
            this.Transaction.Rollback();
        }

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        /// <param name="disposing">A value indicating whether to dispose of managed resources.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.Transaction != null)
                    {
                        this.Transaction.Dispose();
                        this.Transaction = null;
                    }

                    if (this.Connection != null)
                    {
                        this.Connection.Dispose();
                        this.Connection = null;
                    }
                }

                this.disposed = true;
            }
        }
    }
}
