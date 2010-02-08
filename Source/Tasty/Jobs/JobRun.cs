//-----------------------------------------------------------------------
// <copyright file="JobRun.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    /// Represents a single job run.
    /// </summary>
    internal sealed class JobRun
    {
        #region Private Fields

        private Thread executionThread;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the JobRun class.
        /// </summary>
        /// <param name="jobId">The ID of the job being run.</param>
        /// <param name="job">The job to run.</param>
        public JobRun(int jobId, IJob job)
        {
            if (jobId < 1)
            {
                throw new ArgumentException("jobId must be greater than 0.", "jobId");
            }

            if (job == null)
            {
                throw new ArgumentNullException("job", "job must have a value.");
            }

            this.JobId = jobId;
            this.Job = job;
        }

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets an exception that occurred during job execution, if applicable.
        /// </summary>
        public Exception ExecutionException { get; private set; }

        /// <summary>
        /// Gets the date the job finished executing, if applicable.
        /// </summary>
        public DateTime Finished { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the job is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets the job being run.
        /// </summary>
        public IJob Job { get; private set; }

        /// <summary>
        /// Gets the ID of the job being run.
        /// </summary>
        public int JobId { get; private set; }

        /// <summary>
        /// Gets the date the job run was started, if applicable.
        /// </summary>
        public DateTime Started { get; private set; }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Aborts the job if it is running.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Abort is a best-try call; we don't want to kill the calling thread if it fails for any reason.")]
        public void Abort()
        {
            lock (this)
            {
                if (this.IsRunning)
                {
                    this.IsRunning = false;
                    this.Finished = DateTime.UtcNow;

                    try
                    {
                        if (this.executionThread.IsAlive)
                        {
                            this.executionThread.Abort();
                        }
                    }
                    catch
                    {
                        // Eat it.
                    }
                }
            }
        }

        /// <summary>
        /// Runs the job if it has not already been run and it is not currently running.
        /// </summary>
        public void Run()
        {
            lock (this)
            {
                if (!this.IsRunning && this.Finished == DateTime.MinValue)
                {
                    this.IsRunning = true;
                    this.Started = DateTime.UtcNow;

                    this.executionThread = new Thread(this.RunInternal);
                    this.executionThread.Start();
                }
            }
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Internal concrete job execution.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to log any exceptions thrown during the job run instead of dying completely.")]
        private void RunInternal()
        {
            try
            {
                this.Job.Execute();

                lock (this)
                {
                    this.IsRunning = false;
                    this.Finished = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                lock (this)
                {
                    this.ExecutionException = ex;
                    this.IsRunning = false;
                    this.Finished = DateTime.UtcNow;
                }
            }
        }

        #endregion
    }
}