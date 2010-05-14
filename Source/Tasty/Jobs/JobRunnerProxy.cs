//-----------------------------------------------------------------------
// <copyright file="JobRunnerProxy.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;

    /// <summary>
    /// Provides proxy access to the singleton <see cref="JobRunner"/> instance
    /// across application domains.
    /// </summary>
    internal sealed class JobRunnerProxy : MarshalByRefObject
    {
        #region Private Fields

        private bool isGreen = true;

        #endregion

        #region Events

        /// <summary>
        /// Event raised when the job runner is shutting down and all running jobs have finished.
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

        #region Public Instance Properties

        /// <summary>
        /// Gets a value indicating whether the job runner is in the process of being shut down.
        /// </summary>
        public bool IsShuttingDown { get; private set; }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Starts the job runner.
        /// </summary>
        public void StartRunner()
        {
            lock (this)
            {
                if (!this.IsShuttingDown)
                {
                    if (this.isGreen)
                    {
                        JobRunner.Instance.CancelJob += new EventHandler<JobRecordEventArgs>(this.JobRunnerCancelJob);
                        JobRunner.Instance.DequeueJob += new EventHandler<JobRecordEventArgs>(this.JobRunnerDequeueJob);
                        JobRunner.Instance.EnqueueScheduledJob += new EventHandler<JobRecordEventArgs>(this.JobRunnerEnqueueScheduledJob);
                        JobRunner.Instance.Error += new EventHandler<JobErrorEventArgs>(this.JobRunnerError);
                        JobRunner.Instance.FinishJob += new EventHandler<JobRecordEventArgs>(this.JobRunnerFinishJob);
                        JobRunner.Instance.TimeoutJob += new EventHandler<JobRecordEventArgs>(this.JobRunnerTimeoutJob);

                        this.isGreen = false;
                    }

                    JobRunner.Instance.Start();
                }
            }
        }

        /// <summary>
        /// Stops the job runner by issuing a stop command and firing
        /// an <see cref="AllFinished"/> event once all running jobs have finished executing.
        /// </summary>
        public void StopRunner()
        {
            lock (this)
            {
                if (!this.IsShuttingDown)
                {
                    this.IsShuttingDown = true;

                    JobRunner.Instance.Stop(delegate
                    {
                        this.IsShuttingDown = false;

                        if (this.AllFinished != null)
                        {
                            this.AllFinished(this, EventArgs.Empty);
                        }
                    });
                }
            }
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Raises the JobRunner.Instance's CancelJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void JobRunnerCancelJob(object sender, JobRecordEventArgs e)
        {
            if (this.CancelJob != null)
            {
                this.CancelJob(this, e);
            }
        }

        /// <summary>
        /// Raises the JobRunner.Instance's DequeueJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void JobRunnerDequeueJob(object sender, JobRecordEventArgs e)
        {
            if (this.DequeueJob != null)
            {
                this.DequeueJob(this, e);
            }
        }

        /// <summary>
        /// Raises the JobRunner.Instance's EnqueueScheduledJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void JobRunnerEnqueueScheduledJob(object sender, JobRecordEventArgs e)
        {
            if (this.EnqueueScheduledJob != null)
            {
                this.EnqueueScheduledJob(this, e);
            }
        }

        /// <summary>
        /// Raises the JobRunner.Instance's Error event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void JobRunnerError(object sender, JobErrorEventArgs e)
        {
            if (this.Error != null)
            {
                this.Error(this, e);
            }
        }

        /// <summary>
        /// Raises the JobRunner.Instance's FinishJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void JobRunnerFinishJob(object sender, JobRecordEventArgs e)
        {
            if (this.FinishJob != null)
            {
                this.FinishJob(this, e);
            }
        }

        /// <summary>
        /// Raises the JobRunner.Instance's TimeoutJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void JobRunnerTimeoutJob(object sender, JobRecordEventArgs e)
        {
            if (this.TimeoutJob != null)
            {
                this.TimeoutJob(this, e);
            }
        }

        #endregion
    }
}
