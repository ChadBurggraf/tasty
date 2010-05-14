//-----------------------------------------------------------------------
// <copyright file="JobRunner.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
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
        private static JobRunner instance;
        private Thread god;
        private IList<JobRun> runningJobs;
        private Action onAllFinished;

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
            this.IsGreen = true;
        }

        #endregion

        #region Events

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
        /// Gets the singleton job runner instance.
        /// </summary>
        public static JobRunner Instance
        {
            get
            {
                lock (instanceLocker)
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
        /// Gets the number of jobs currently being executed by the runner.
        /// This number may reflect jobs that have finished but have yet to
        /// be flushed.
        /// </summary>
        public int ExecutingJobCount
        {
            get
            {
                return this.runningJobs.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the runner has never been started
        /// since the application context was created. Returns true if it has
        /// never been started, returns fals if it has been started at least once.
        /// </summary>
        public bool IsGreen { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the runner is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Starts the runner if it is not already running.
        /// </summary>
        public void Start()
        {
            lock (this.statusLocker)
            {
                this.IsRunning = true;

                if (this.IsGreen)
                {
                    this.IsGreen = false;
                    
                    this.god = new Thread(this.SmiteThee);
                    this.god.Start();
                }
            }
        }

        /// <summary>
        /// Stops the runner if it is running.
        /// Does not abort any currently executing job runs.
        /// </summary>
        public void Stop()
        {
            this.Stop(null);
        }

        /// <summary>
        /// Stops the runner if it is running.
        /// Does not abort any currently executing job runs.
        /// </summary>
        /// <param name="onAllFinished">A callback to invoke when all currently-running jobs have finished or timed out.</param>
        public void Stop(Action onAllFinished)
        {
            lock (this.statusLocker)
            {
                this.onAllFinished = onAllFinished;
                this.IsRunning = false;
            }
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
                JobStore.Current.CancelingJobs(
                    runningIds, 
                    delegate(IEnumerable<JobRecord> records)
                    {
                        JobStore.Current.UpdateJobs(
                            records, 
                            delegate(JobRecord record)
                            {
                                JobRun run = this.runningJobs.Where(j => j.JobId == record.Id.Value).FirstOrDefault();
                                this.runningJobs.Remove(run);

                                run.Abort();

                                record.Status = JobStatus.Canceled;
                                record.FinishDate = DateTime.UtcNow;

                                if (this.CancelJob != null)
                                {
                                    this.CancelJob(this, new JobRecordEventArgs(new JobRecord(record)));
                                }
                            });
                    });
            }
        }

        /// <summary>
        /// Dequeues pending jobs in the job store.
        /// </summary>
        private void DequeueJobs()
        {
            if (this.runningJobs.Count < TastySettings.Section.Jobs.MaximumConcurrency)
            {
                JobStore.Current.DequeueingJobs(
                    TastySettings.Section.Jobs.MaximumConcurrency - this.runningJobs.Count, 
                    delegate(IEnumerable<JobRecord> records)
                    {
                        JobStore.Current.UpdateJobs(
                            records,
                            delegate(JobRecord record)
                            {
                                if (record.Status != JobStatus.Failed)
                                {
                                    record.Status = JobStatus.Started;
                                    record.StartDate = DateTime.UtcNow;

                                    IJob job = null;
                                    Exception toJobEx = null;

                                    try
                                    {
                                        job = record.ToJob();
                                    }
                                    catch (FileLoadException ex)
                                    {
                                        toJobEx = ex;
                                    }
                                    catch (FileNotFoundException ex)
                                    {
                                        toJobEx = ex;
                                    }
                                    catch (SerializationException ex)
                                    {
                                        toJobEx = ex;
                                    }

                                    if (toJobEx == null)
                                    {
                                        JobRun run = new JobRun(record.Id.Value, job);
                                        run.Run();

                                        this.runningJobs.Add(run);
                                    }
                                    else
                                    {
                                        record.Status = JobStatus.Failed;
                                        record.Exception = new ExceptionXElement(toJobEx).ToString();
                                        record.FinishDate = DateTime.UtcNow;

                                        if (this.Error != null)
                                        {
                                            this.Error(this, new JobErrorEventArgs(new JobRecord(record), toJobEx));
                                        }
                                    }
                                }
                                else
                                {
                                    record.FinishDate = DateTime.UtcNow;
                                }

                                if (this.DequeueJob != null)
                                {
                                    this.DequeueJob(this, new JobRecordEventArgs(new JobRecord(record)));
                                }
                            });
                    });
            }
        }

        /// <summary>
        /// Enqueues any scheduled jobs that are either new to the system or
        /// need to be re-enqueued due to their next scheduled run date arriving.
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
                                where r.JobType != null && r.JobType.AssemblyQualifiedName.StartsWith(scheduledJob.JobType, StringComparison.OrdinalIgnoreCase) && r.ScheduleName == schedule.Name
                                select r).FirstOrDefault();

                    if (last == null || last.QueueDate < next)
                    {
                        try
                        {
                            IJob job = ScheduledJob.CreateFromConfiguration(scheduledJob);
                            JobRecord record = job.Enqueue(next, schedule.Name);

                            if (this.EnqueueScheduledJob != null)
                            {
                                this.EnqueueScheduledJob(this, new JobRecordEventArgs(new JobRecord(record)));
                            }
                        }
                        catch (ConfigurationErrorsException ex)
                        {
                            if (this.Error != null)
                            {
                                this.Error(this, new JobErrorEventArgs(new JobRecord(), ex));
                            }
                        }
                    }
                    else if (TastySettings.Section.Jobs.DeleteBadScheduledJobRecords || TastySettings.Section.Jobs.NotifyOnBadScheduledJobs)
                    {
                        var bad = from r in records
                                  where r.JobType == null && r.ScheduleName == schedule.Name
                                  select r;

                        foreach (JobRecord badRecord in bad)
                        {
                            if (TastySettings.Section.Jobs.DeleteBadScheduledJobRecords)
                            {
                                JobStore.Current.DeleteJob(badRecord.Id.Value);
                            }

                            if (TastySettings.Section.Jobs.NotifyOnBadScheduledJobs)
                            {
                                if (this.Error != null)
                                {
                                    this.Error(this, new JobErrorEventArgs(new JobRecord(badRecord), null));
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finishes any jobs that have completed by updating their records in the job store.
        /// </summary>
        private void FinishJobs()
        {
            var finishedIds = (from r in this.runningJobs
                               where !r.IsRunning
                               select r.JobId).ToArray();

            if (finishedIds.Length > 0)
            {
                JobStore.Current.FinishingJobs(
                    finishedIds, 
                    delegate(IEnumerable<JobRecord> records)
                    {
                        JobStore.Current.UpdateJobs(
                            records, 
                            delegate(JobRecord record)
                            {
                                JobRun run = this.runningJobs.Where(j => j.JobId == record.Id.Value).FirstOrDefault();
                                this.runningJobs.Remove(run);

                                record.FinishDate = run.Finished;

                                if (run.ExecutionException != null)
                                {
                                    record.Exception = new ExceptionXElement(run.ExecutionException).ToString();
                                    record.Status = JobStatus.Failed;

                                    if (this.Error != null)
                                    {
                                        this.Error(this, new JobErrorEventArgs(new JobRecord(record), run.ExecutionException));
                                    }
                                }
                                else
                                {
                                    record.Status = JobStatus.Succeeded;
                                }

                                if (this.FinishJob != null)
                                {
                                    this.FinishJob(this, new JobRecordEventArgs(new JobRecord(record)));
                                }
                            });
                    });
            }
        }

        /// <summary>
        /// God execution thread handler.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to keep the run loop executing at all costs.")]
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
                            this.EnqueueScheduledJobs();
                            this.DequeueJobs();
                        }
                        else if (this.onAllFinished != null && this.runningJobs.Count == 0)
                        {
                            this.onAllFinished();
                            this.onAllFinished = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (this.Error != null)
                    {
                        this.Error(this, new JobErrorEventArgs(new JobRecord(), ex));
                    }
                }

                Thread.Sleep(TastySettings.Section.Jobs.Heartbeat);
            }
        }

        /// <summary>
        /// Times out any currently running jobs that have been running for too long.
        /// </summary>
        private void TimeoutJobs()
        {
            var timedOutIds = (from r in this.runningJobs
                               where r.IsRunning && DateTime.UtcNow.Subtract(r.Started).TotalMilliseconds > r.Job.Timeout
                               select r.JobId).ToArray();

            if (timedOutIds.Length > 0)
            {
                JobStore.Current.TimingOutJobs(
                    timedOutIds, 
                    delegate(IEnumerable<JobRecord> records)
                    {
                        JobStore.Current.UpdateJobs(
                            records, 
                            delegate(JobRecord record)
                            {
                                JobRun run = this.runningJobs.Where(j => j.JobId == record.Id.Value).FirstOrDefault();
                                this.runningJobs.Remove(run);

                                run.Abort();

                                record.Status = JobStatus.TimedOut;
                                record.FinishDate = DateTime.UtcNow;

                                if (this.TimeoutJob != null)
                                {
                                    this.TimeoutJob(this, new JobRecordEventArgs(new JobRecord(record)));
                                }
                            });
                    });
            }
        }

        #endregion
    }
}