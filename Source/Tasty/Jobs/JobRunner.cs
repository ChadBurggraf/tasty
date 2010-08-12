//-----------------------------------------------------------------------
// <copyright file="JobRunner.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Tasty.Configuration;

    /// <summary>
    /// Runs jobs.
    /// </summary>
    public sealed class JobRunner
    {
        #region Private Fields

        private static readonly object instanceLocker = new object();
        private readonly object statusLocker = new object();
        private static Dictionary<string, JobRunner> instances = new Dictionary<string, JobRunner>();
        private RunningJobs runs;
        private IJobStore store;
        private Thread god;

        #endregion

        #region Construction

        /// <summary>
        /// Prevents a default instance of the JobRunner class from being created.
        /// </summary>
        /// <param name="store">The <see cref="IJobStore"/> to use when accessing job data.</param>
        private JobRunner(IJobStore store)
        {
            if (TastySettings.Section.Jobs.Heartbeat < 1)
            {
                throw new InvalidOperationException("The configured job heartbeat must be greater than 0.");
            }

            this.runs = new RunningJobs(Path.Combine(Environment.CurrentDirectory, RunningJobs.GeneratePersistenceFileName(store)));
            this.store = store;
        }

        #endregion

        #region Events

        /// <summary>
        /// Event raised when the runner has finished safely shutting down
        /// there are no jobs currently running.
        /// </summary>
        public event EventHandler AllFinished;

        /// <summary>
        /// Event raised when a job has been canceled and and its run terminated.
        /// </summary>
        public event EventHandler<JobRecordEventArgs> CancelJob;

        /// <summary>
        /// Event raised when a job has been dequeued from the persistent store
        /// and its run started.
        /// </summary>
        public event EventHandler<JobRecordEventArgs> DequeueJob;

        /// <summary>
        /// Event raised when an error occurs.
        /// WARNING: The <see cref="JobRecord"/> passed with this event may be empty,
        /// or the <see cref="Exception"/> may be null, or both.
        /// </summary>
        public event EventHandler<JobErrorEventArgs> Error;

        /// <summary>
        /// Event raised when a job has finished execution.
        /// This event is raised when the job finished naturally (i.e., not by canceling or timing out),
        /// but regardless of whether it succeeded or not.
        /// </summary>
        public event EventHandler<JobRecordEventArgs> FinishJob;

        /// <summary>
        /// Event raised when a job has been timed out.
        /// </summary>
        public event EventHandler<JobRecordEventArgs> TimeoutJob;

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets the number of jobs currently being executed by the runner.
        /// This number may reflect jobs that have finished but have yet to
        /// be flushed.
        /// </summary>
        public int ExecutingJobCount
        {
            get { return this.runs.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the runner is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the running is in the process of shutting down.
        /// </summary>
        public bool IsShuttingDown { get; private set; }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Gets the <see cref="JobRunner"/> instance for the <see cref="IJobStore"/> returned by <see cref="JobStore.Current"/>.
        /// </summary>
        /// <returns>A <see cref="JobRunner"/> instance.</returns>
        public static JobRunner GetInstance()
        {
            return GetInstance(JobStore.Current);
        }

        /// <summary>
        /// Gets the <see cref="JobRunner"/> instance for the specified <see cref="IJobStore"/>.
        /// </summary>
        /// <param name="store">The <see cref="IJobStore"/> to get the <see cref="JobRunner"/> instance for.</param>
        /// <returns>A <see cref="JobRunner"/> instance.</returns>
        public static JobRunner GetInstance(IJobStore store)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store", "store cannot be null.");
            }

            lock (instanceLocker)
            {
                if (!instances.ContainsKey(store.TypeKey))
                {
                    instances[store.TypeKey] = new JobRunner(store);
                }

                return instances[store.TypeKey];
            }
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Pauses the job runner by preventing any new jobs
        /// from being dequeued. Does not abort any currently running jobs.
        /// </summary>
        public void Pause()
        {
            lock (this.statusLocker)
            {
                this.IsRunning = false;
            }
        }

        /// <summary>
        /// Starts the runner if it is not already running.
        /// </summary>
        public void Start()
        {
            lock (this.statusLocker)
            {
                if (!this.IsShuttingDown)
                {
                    this.IsRunning = true;

                    if (this.god == null || !this.god.IsAlive)
                    {
                        this.god = new Thread(this.SmiteThee);
                        this.god.Start();
                    }
                }
            }
        }

        /// <summary>
        /// Stops the runner if it is running.
        /// </summary>
        /// <param name="safely">A value indicating whether to safely shut down, allowing all currently
        /// running jobs to complete, or immediately terminate all running jobs.</param>
        public void Stop(bool safely)
        {
            lock (this.statusLocker)
            {
                if (!safely || !this.IsShuttingDown)
                {
                    this.IsShuttingDown = true;
                    this.IsRunning = false;

                    if (!safely)
                    {
                        if (this.god != null && this.god.IsAlive)
                        {
                            try
                            {
                                this.god.Abort();
                                this.god = null;
                            }
                            catch
                            {
                            }
                        }

                        this.IsShuttingDown = false;
                    }
                }
            }
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Cancels any jobs marked as <see cref="JobStatus.Canceling"/>.
        /// </summary>
        private void CancelJobs()
        {
            var trans = this.store.StartTransaction();

            try
            {
                var runs = this.runs.GetRunning();
                var records = this.store.GetJobs(runs.Select(r => r.JobId), trans);

                var canceling = from run in runs
                                join record in records on run.JobId equals record.Id.Value
                                where record.Status == JobStatus.Canceling
                                select new
                                {
                                    Run = run,
                                    Record = record
                                };

                foreach (var job in canceling)
                {
                    job.Run.Abort();
                    job.Record.Status = JobStatus.Canceled;
                    job.Record.FinishDate = job.Run.FinishDate;

                    this.store.SaveJob(job.Record, trans);
                    this.runs.Remove(job.Record.Id.Value);

                    this.RaiseEvent(this.CancelJob, new JobRecordEventArgs(job.Record));
                }

                this.runs.Flush();
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Dequeues pending jobs in the job store.
        /// </summary>
        private void DequeueJobs()
        {
            int count = TastySettings.Section.Jobs.MaximumConcurrency - this.ExecutingJobCount;

            if (count > 0)
            {
                var trans = this.store.StartTransaction();

                try
                {
                    foreach (var record in this.store.GetJobs(JobStatus.Queued, count, trans))
                    {
                        record.Status = JobStatus.Started;
                        record.StartDate = DateTime.UtcNow;

                        IJob job = null;
                        Exception toJobEx = null;

                        try
                        {
                            job = record.ToJob();
                        }
                        catch (InvalidOperationException ex)
                        {
                            toJobEx = ex.InnerException ?? ex;
                            this.RaiseEvent(this.Error, new JobErrorEventArgs(record, toJobEx));
                        }

                        if (job != null)
                        {
                            JobRun run = new JobRun(record.Id.Value, job);
                            this.runs.Add(run);

                            run.Start();
                        }
                        else
                        {
                            record.Status = JobStatus.FailedToLoadType;
                            record.FinishDate = DateTime.UtcNow;
                            record.Exception = new ExceptionXElement(toJobEx).ToString();
                        }

                        this.store.SaveJob(record, trans);
                        this.RaiseEvent(this.DequeueJob, new JobRecordEventArgs(record));
                    }

                    this.runs.Flush();
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Finishes any jobs that have completed by updating their records in the job store.
        /// </summary>
        private void FinishJobs()
        {
            var trans = this.store.StartTransaction();

            try
            {
                var runs = this.runs.GetNotRunning();
                var records = this.store.GetJobs(runs.Select(r => r.JobId), trans);

                var finishing = from run in runs
                                join record in records on run.JobId equals record.Id.Value
                                where record.Status == JobStatus.Started
                                select new
                                {
                                    Run = run,
                                    Record = record
                                };

                foreach (var job in finishing)
                {
                    job.Record.FinishDate = job.Run.FinishDate;

                    if (job.Run.ExecutionException != null)
                    {
                        job.Record.Exception = new ExceptionXElement(job.Run.ExecutionException).ToString();
                        job.Record.Status = JobStatus.Failed;

                        this.RaiseEvent(this.Error, new JobErrorEventArgs(job.Record, job.Run.ExecutionException));
                    }
                    else if (job.Run.WasRecovered)
                    {
                        job.Record.Status = JobStatus.Interrupted;
                    }
                    else
                    {
                        job.Record.Status = JobStatus.Succeeded;
                    }

                    this.store.SaveJob(job.Record, trans);
                    this.runs.Remove(job.Record.Id.Value);

                    this.RaiseEvent(this.FinishJob, new JobRecordEventArgs(job.Record));
                }

                this.runs.Flush();
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Main God thread run loop.
        /// </summary>
        private void SmiteThee()
        {
            while (true)
            {
                try
                {
                    //this.CancelJobs();
                    this.DequeueJobs();
                    /*
                    //this.TimeoutJobs();
                    //this.FinishJobs();

                    lock (this.statusLocker)
                    {
                        if (this.IsRunning)
                        {
                            this.DequeueJobs();
                        }
                        else if (this.IsShuttingDown && this.ExecutingJobCount == 0)
                        {
                            this.IsShuttingDown = false;
                            this.RaiseEvent(this.AllFinished, EventArgs.Empty);
                        }
                    }*/
                }
                catch (Exception ex)
                {
                    throw;
                    //this.RaiseEvent(this.Error, new JobErrorEventArgs(null, ex));
                }

                Thread.Sleep(TastySettings.Section.Jobs.Heartbeat);
            }
        }

        /// <summary>
        /// Times out any currently running jobs that have been running for too long.
        /// </summary>
        private void TimeoutJobs()
        {
            var trans = this.store.StartTransaction();

            try
            {
                var runs = this.runs.GetRunning();
                var records = this.store.GetJobs(runs.Select(r => r.JobId), trans);

                var timingOut = from run in runs
                                join record in records on run.JobId equals record.Id.Value
                                where run.Job != null 
                                    && run.StartDate != null 
                                    && DateTime.UtcNow.Subtract(run.StartDate.Value).TotalMilliseconds >= run.Job.Timeout
                                    && record.Status == JobStatus.Started
                                select new
                                {
                                    Run = run,
                                    Record = record
                                };

                foreach (var job in timingOut)
                {
                    if (job.Run.Abort())
                    {
                        job.Record.Status = JobStatus.TimedOut;
                        job.Record.FinishDate = job.Run.FinishDate;

                        this.store.SaveJob(job.Record, trans);
                        this.runs.Remove(job.Record.Id.Value);

                        this.RaiseEvent(this.TimeoutJob, new JobRecordEventArgs(job.Record));
                    }
                }

                this.runs.Flush();
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        #endregion
    }
}
