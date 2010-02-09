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
            this.IsGreen = true;
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
            lock (this)
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
            this.IsRunning = false;
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Enqueues any scheduled jobs that are either new to the system or
        /// need to be re-enqueued due to their next scheduled run date arriving.
        /// </summary>
        private static void EnqueueScheduledJobs()
        {
            DateTime now = DateTime.UtcNow;
            var records = JobStore.Current.GetLatestScheduledJobs().ToArray();

            foreach (var schedule in TastySettings.Section.Jobs.Schedules)
            {
                DateTime next = ScheduledJob.GetNextExecuteDate(schedule, now);

                foreach (var scheduledJob in schedule.ScheduledJobs)
                {
                    var last = (from r in records
                                where r.JobType.AssemblyQualifiedName == scheduledJob.JobType && r.ScheduleName == schedule.Name
                                select r).FirstOrDefault();

                    if (last == null || last.QueueDate < next)
                    {
                        try
                        {
                            IJob job = ScheduledJob.CreateFromConfiguration(scheduledJob);
                            job.Enqueue(next, schedule.Name);
                        }
                        catch (ConfigurationErrorsException)
                        {
                            // TODO: Something should be done with this.
                        }
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
                                run.Abort();
                                this.runningJobs.Remove(run);

                                record.Status = JobStatus.Canceled;
                                record.FinishDate = run.Finished;
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
                                record.Status = JobStatus.Started;
                                record.StartDate = DateTime.UtcNow;

                                IJob job = null;

                                try
                                {
                                    job = record.ToJob();
                                }
                                catch (SerializationException ex)
                                {
                                    record.Status = JobStatus.Failed;
                                    record.Exception = new ExceptionXElement(ex).ToString();
                                    record.FinishDate = DateTime.UtcNow;

                                    return;
                                }

                                JobRun run = new JobRun(record.Id.Value, job);
                                this.runningJobs.Add(run);

                                run.Run();
                            });
                    });
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
                                }
                                else
                                {
                                    record.Status = JobStatus.Succeeded;
                                }
                            });
                    });
            }
        }

        /// <summary>
        /// God execution thread handler.
        /// </summary>
        private void SmiteThee()
        {
            while (true)
            {
                this.CancelJobs();
                this.FinishJobs();
                this.TimeoutJobs();

                if (this.IsRunning)
                {
                    EnqueueScheduledJobs();
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
                                run.Abort();
                                this.runningJobs.Remove(run);

                                record.Status = JobStatus.TimedOut;
                                record.FinishDate = DateTime.UtcNow;
                            });
                    });
            }
        }

        #endregion
    }
}