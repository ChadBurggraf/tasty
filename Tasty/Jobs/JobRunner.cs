//-----------------------------------------------------------------------
// <copyright file="JobRunner.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Tasty.Configuration;

    /// <summary>
    /// Runs jobs.
    /// </summary>
    public sealed class JobRunner
    {
        #region Private Fields

        private static readonly object locker = new object();
        private static JobRunner instance;
        private Thread god;
        private IList<JobRun> runningJobs;

        #endregion

        #region Construction

        /// <summary>
        /// Prevents a default instance of the JobRunner class from being created.
        /// </summary>
        private JobRunner()
        {
            if (TastySettings.Section.Jobs.Heartbeat < 1)
            {
                throw new InvalidOperationException("The configured job heartbeat must be greater than 0.");
            }

            this.runningJobs = new List<JobRun>();
            this.god = new Thread(this.SmiteThee);
            this.god.Start();
        }

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Gets the singleton job runner instance.
        /// </summary>
        public static JobRunner Instance
        {
            get
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new JobRunner();
                    }

                    return instance;
                }
            }
        }

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets a value indicating whether the runner is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Starts the running if it is not already running.
        /// </summary>
        public void Start()
        {
            this.IsRunning = true;
        }

        /// <summary>
        /// Stops the runner if it is running.
        /// Does not abort any currently executing job runs.
        /// </summary>
        public void Stop()
        {
            this.IsRunning = false;
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Cancels any jobs marked as <see cref="JobStatus.Canceling"/>.
        /// </summary>
        private void CancelJobs()
        {
            var runningIds = (from r in this.runningJobs
                              where r.IsRunning
                              select r.JobId).ToArray();

            if (runningIds.Length > 0)
            {
                Job.ConfiguredStore.CancelingJobs(runningIds, delegate(IEnumerable<JobRecord> records)
                {
                    var canceling = from record in records
                                    join run in this.runningJobs on record.Id equals run.JobId
                                    select new
                                    {
                                        Record = record,
                                        Run = run
                                    };

                    foreach (var cancel in canceling)
                    {
                        cancel.Run.Abort();
                        this.runningJobs.Remove(cancel.Run);

                        cancel.Record.Status = JobStatus.Canceled;
                        cancel.Record.FinishDate = cancel.Run.Finished;

                        Job.ConfiguredStore.UpdateJobs(new JobRecord[] { cancel.Record }, null);
                    }
                });
            }
        }

        /// <summary>
        /// Dequeues pending jobs in the job store.
        /// </summary>
        private void DequeueJobs()
        {
            if (TastySettings.Section.Jobs.MaximumConcurrency < this.runningJobs.Count)
            {
                Job.ConfiguredStore.DequeueJobs(
                    delegate(IEnumerable<JobRecord> records)
                    {
                        Job.ConfiguredStore.UpdateJobs(
                            records, 
                            delegate(JobRecord record)
                            {
                                record.Status = JobStatus.Started;
                                record.StartDate = DateTime.UtcNow;

                                JobRun run = new JobRun(record.Id.Value, record.ToJob());
                                this.runningJobs.Add(run);

                                run.Run();
                            });
                    }, 
                    TastySettings.Section.Jobs.MaximumConcurrency - this.runningJobs.Count);
            }
        }

        /// <summary>
        /// Finishes any jobs that have completed by updating their records in the job store.
        /// </summary>
        private void FinishJobs()
        {
            var runs = (from r in this.runningJobs
                        where !r.IsRunning
                        select r).ToArray();

            if (runs.Length > 0)
            {

            }
        }

        /// <summary>
        /// God execution thread handler.
        /// </summary>
        private void SmiteThee()
        {
            while (true)
            {
                if (this.IsRunning)
                {
                    this.CancelJobs();
                    this.FinishJobs();
                    this.TimeoutJobs();
                    this.DequeueJobs();
                }

                Thread.Sleep(TastySettings.Section.Jobs.Heartbeat);
            }
        }

        /// <summary>
        /// Times out any currently running jobs that have been running for too long.
        /// </summary>
        private void TimeoutJobs()
        {
        }

        #endregion
    }
}