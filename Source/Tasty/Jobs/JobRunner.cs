

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Xml;
    using Tasty.Configuration;

    /// <summary>
    /// Runs jobs.
    /// </summary>
    internal sealed class JobRunner
    {
        #region Private Fields

        private static readonly object currentLocker = new object();
        private readonly object statusLocker = new object();
        private static JobRunner current;
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
        /// Event raised when a scheduled job has been enqueued to the persistent store.
        /// </summary>
        public event EventHandler<JobRecordEventArgs> EnqueueScheduledJob;

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

        #region Public Static Properties

        /// <summary>
        /// Gets the current <see cref="JobRunner"/> instance.
        /// </summary>
        public static JobRunner Current
        {
            get
            {
                lock (currentLocker)
                {
                    return current ?? (current = new JobRunner());
                }
            }
        }

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets the number of jobs currently being executed.
        /// </summary>
        public int ExecutingJobCount
        {
            get { return this.runningJobs.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the runner is running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the runner is in the process of shutting down.
        /// </summary>
        public bool IsShuttingDown { get; private set; }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Pauses the runner. Lets currently-running jobs complete, but does not
        /// dequeue any new jobs from the job store and does not enqueue any new
        /// scheduled jobs.
        /// </summary>
        public void Pause()
        {
            lock (statusLocker)
            {
                this.IsRunning = false;
            }
        }

        /// <summary>
        /// Starts the runner if it is not already running.
        /// </summary>
        public void Start()
        {
            lock (statusLocker)
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
        /// Stops the runner, optionally aborting any currently-running jobs.
        /// </summary>
        /// <param name="safely">A value indicating whether to safely shut down by allowing currently-running jobs to complete or timeout.</param>
        public void Stop(bool safely)
        {
            lock (statusLocker)
            {
                if (!this.IsShuttingDown)
                {
                    this.IsShuttingDown = true;
                    this.IsRunning = false;

                    if (!safely && this.god != null && this.god.IsAlive)
                    {
                        try
                        {
                            this.god.Abort();
                        }
                        catch (Exception ex)
                        {
                            this.RaiseEvent(this.Error, new JobErrorEventArgs(null, ex));
                        }

                        this.god = null;
                        this.IsShuttingDown = false;
                    }
                }
            }
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Creates a new <see cref="IJob"/> instance of the type specified by the given record.
        /// </summary>
        /// <param name="record">The record to create the <see cref="IJob"/> instance for.</param>
        /// <returns>An <see cref="IJob"/> instance.</returns>
        private static IJob CreateJobInstance(JobRecord record)
        {
            DataContractSerializer serializer = new DataContractSerializer(Type.GetType(record.JobType));

            using (StringReader sr = new StringReader(record.Data))
            {
                using (XmlReader xr = new XmlTextReader(sr))
                {
                    return (IJob)serializer.ReadObject(xr);
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="IJob"/> instance from a configured scheduled job.
        /// </summary>
        /// <param name="element">The configuration element to create the <see cref="IJob"/> instance for.</param>
        /// <returns>An <see cref="IJob"/> instance.</returns>
        private static IJob CreateScheduledJobInstance(JobScheduledJobElement element)
        {
            IJob job = null;

            try
            {
                job = (IJob)Activator.CreateInstance(Type.GetType(element.JobType));
                ScheduledJob sj = job as ScheduledJob;

                if (sj != null)
                {
                    sj.Metadata.FillWith(element.Metadata);
                }
            }
            catch (Exception ex)
            {
                throw new ConfigurationException(String.Format(CultureInfo.InvariantCulture, "The type \"{0}\" could not be instantiated into an object implementing the Tasty.Jobs.IJob interface.", element.JobType), ex);
            }

            return job;
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Cancels any jobs marked as <see cref="JobStatus.Canceling"/> in the job store.
        /// </summary>
        private void CancelJobs()
        {
            lock (this.runningJobs)
            {
                var runningIds = (from r in this.runningJobs
                                  where r.IsRunning
                                  select r.JobId).ToArray();

                if (runningIds.Length > 0)
                {
                    JobStore.Current.StartTransaction();

                    try
                    {
                        var records = JobStore.Current.GetJobs(runningIds);

                        foreach (var record in records)
                        {
                            try
                            {
                                JobRun run = this.runningJobs.Where(r => r.JobId == record.Id).FirstOrDefault();
                                this.runningJobs.Remove(run);

                                run.Abort();

                                record.Status = JobStatus.Canceled;
                                record.FinishDate = run.FinishDate.Value;

                                JobStore.Current.SaveJob(record);
                                this.RaiseEvent(this.CancelJob, new JobRecordEventArgs(record));
                            }
                            catch (Exception ex)
                            {
                                this.RaiseEvent(this.Error, new JobErrorEventArgs(record, ex));
                            }
                        }

                        JobStore.Current.CommitTransaction();
                    }
                    catch
                    {
                        JobStore.Current.RollbackTransaction();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Dequeues pending jobs from the job store and starts them.
        /// </summary>
        private void DequeueJobs()
        {
            lock (this.runningJobs)
            {
                if (this.runningJobs.Count < TastySettings.Section.Jobs.MaximumConcurrency)
                {
                    JobStore.Current.StartTransaction();

                    try
                    {
                        var records = JobStore.Current.GetJobs(JobStatus.Queued, TastySettings.Section.Jobs.MaximumConcurrency - this.runningJobs.Count);

                        foreach (var record in records)
                        {
                            try
                            {
                                IJob job = null;
                                Exception toJobEx = null;

                                try
                                {
                                    job = CreateJobInstance(record);
                                }
                                catch (Exception ex)
                                {
                                    toJobEx = ex;
                                }

                                if (toJobEx == null)
                                {
                                    JobRun run = new JobRun(record.Id.Value, job);
                                    run.Finished += new EventHandler<JobRunEventArgs>(OnJobRunFinsiehd);
                                    run.Run();

                                    record.Status = JobStatus.Started;
                                    record.StartDate = run.StartDate.Value;

                                    this.runningJobs.Add(run);
                                }
                                else
                                {
                                    record.Status = JobStatus.FailedToLoadType;
                                    record.Exception = new ExceptionXElement(toJobEx).ToString();
                                    record.FinishDate = DateTime.UtcNow;

                                    this.RaiseEvent(this.Error, new JobErrorEventArgs(record, toJobEx));
                                }

                                JobStore.Current.SaveJob(record);

                                if (toJobEx == null)
                                {
                                    this.RaiseEvent(this.DequeueJob, new JobRecordEventArgs(record));
                                }
                            }
                            catch (Exception ex)
                            {
                                this.RaiseEvent(this.Error, new JobErrorEventArgs(record, ex));
                            }
                        }

                        JobStore.Current.CommitTransaction();
                    }
                    catch
                    {
                        JobStore.Current.RollbackTransaction();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Enqueues upcoming scheduled jobs into the job store.
        /// </summary>
        private void EnqueueScheduledJobs()
        {
            DateTime now = DateTime.UtcNow;
            var records = JobStore.Current.GetLatestScheduledJobs().ToArray();

            foreach (var schedule in TastySettings.Section.Jobs.Schedules)
            {
                DateTime next = ScheduledJob.GetNextExecuteDate(schedule, now);

                foreach (var scheduledJob in schedule.ScheduledJobs)
                {
                    var last = (from r in records
                                where r.ScheduleName == schedule.Name && r.JobType.StartsWith(scheduledJob.JobType, StringComparison.OrdinalIgnoreCase)
                                select r).FirstOrDefault();

                    if (last == null || last.QueueDate < next)
                    {
                        JobRecord record = CreateScheduledJobInstance(scheduledJob).Enqueue(next, schedule.Name);
                        this.RaiseEvent(this.EnqueueScheduledJob, new JobRecordEventArgs(record));
                    }
                }
            }
        }

        /// <summary>
        /// Finishes any in-memory jobs that have finished running.
        /// </summary>
        private void FinishJobs()
        {
            lock (this.runningJobs)
            {
                var finishedIds = (from r in this.runningJobs
                                   where !r.IsRunning
                                   select r.JobId).ToArray();

                if (finishedIds.Length > 0)
                {
                    JobStore.Current.StartTransaction();

                    try
                    {
                        var records = JobStore.Current.GetJobs(finishedIds);

                        foreach (var record in records)
                        {
                            try
                            {
                                JobRun run = this.runningJobs.Where(r => r.JobId == record.Id).First();
                                this.runningJobs.Remove(run);

                                record.FinishDate = run.FinishDate.Value;

                                if (run.ExecutionException != null)
                                {
                                    record.Exception = new ExceptionXElement(run.ExecutionException).ToString();
                                    record.Status = JobStatus.Failed;
                                }
                                else
                                {
                                    record.Status = JobStatus.Succeeded;
                                }

                                JobStore.Current.SaveJob(record);
                                this.RaiseEvent(this.FinishJob, new JobRecordEventArgs(record));
                            }
                            catch (Exception ex)
                            {
                                this.RaiseEvent(this.Error, new JobErrorEventArgs(record, ex));
                            }
                        }

                        JobStore.Current.CommitTransaction();
                    }
                    catch
                    {
                        JobStore.Current.RollbackTransaction();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Raises a <see cref="JobRun"/>'s Finished event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnJobRunFinsiehd(object sender, JobRunEventArgs e)
        {
            lock (this.runningJobs)
            {
                var run = this.runningJobs.Where(r => r.JobId == e.JobId).FirstOrDefault();

                if (run != null)
                {
                    JobStore.Current.StartTransaction();

                    try
                    {
                        JobRecord record = JobStore.Current.GetJob(run.JobId);

                        if (record != null)
                        {
                            this.runningJobs.Remove(run);
                            record.FinishDate = run.FinishDate.Value;

                            if (run.ExecutionException != null)
                            {
                                record.Exception = new ExceptionXElement(run.ExecutionException).ToString();
                                record.Status = JobStatus.Failed;
                            }
                            else
                            {
                                record.Status = JobStatus.Succeeded;
                            }

                            JobStore.Current.SaveJob(record);
                            this.RaiseEvent(this.FinishJob, new JobRecordEventArgs(record));
                        }

                        JobStore.Current.CommitTransaction();
                    }
                    catch (Exception ex)
                    {
                        JobStore.Current.RollbackTransaction();
                        this.RaiseEvent(this.Error, new JobErrorEventArgs(null, ex));
                    }
                }
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
                    this.CancelJobs();
                    this.FinishJobs();
                    this.TimeoutJobs();

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
                    }
                }
                catch (Exception ex)
                {
                    this.RaiseEvent(this.Error, new JobErrorEventArgs(null, ex));
                }

                Thread.Sleep(TastySettings.Section.Jobs.Heartbeat);
            }
        }

        /// <summary>
        /// Times out any in-memory jobs that have been running too long.
        /// </summary>
        private void TimeoutJobs()
        {
            lock (this.runningJobs)
            {
                var timeoutIds = (from r in this.runningJobs
                                  where r.IsRunning && DateTime.UtcNow.Subtract(r.StartDate.Value).TotalMilliseconds > r.Job.Timeout
                                  select r.JobId).ToArray();

                if (timeoutIds.Length > 0)
                {
                    JobStore.Current.StartTransaction();

                    try
                    {
                        var records = JobStore.Current.GetJobs(timeoutIds);

                        foreach (var record in records)
                        {
                            try
                            {
                                JobRun run = this.runningJobs.Where(r => r.JobId == record.Id).First();
                                this.runningJobs.Remove(run);

                                run.Abort();

                                record.Status = JobStatus.TimedOut;
                                record.FinishDate = run.FinishDate;

                                JobStore.Current.SaveJob(record);
                                this.RaiseEvent(this.TimeoutJob, new JobRecordEventArgs(record));
                            }
                            catch (Exception ex)
                            {
                                this.RaiseEvent(this.Error, new JobErrorEventArgs(record, ex));
                            }
                        }

                        JobStore.Current.CommitTransaction();
                    }
                    catch
                    {
                        JobStore.Current.RollbackTransaction();
                        throw;
                    }
                }
            }
        }

        #endregion
    }
}
