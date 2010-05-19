//-----------------------------------------------------------------------
// <copyright file="JobRunnerEventSink.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;

    /// <summary>
    /// Provides access to <see cref="JobRunner"/> and <see cref="JobRunnerProxy"/>
    /// events across <see cref="AppDomain"/> boundaries.
    /// </summary>
    internal sealed class JobRunnerEventSink : MarshalByRefObject
    {
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

        #region Public Instance Methods

        /// <summary>
        /// Fires <see cref="AllFinished"/> this instance's original <see cref="AppDomain"/>.
        /// </summary>
        public void FireAllFinished()
        {
            if (this.AllFinished != null)
            {
                this.AllFinished(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Fires <see cref="CancelJob"/> this instance's original <see cref="AppDomain"/>.
        /// </summary>
        public void FireCancelJob(JobRecordEventArgs e)
        {
            if (this.CancelJob != null)
            {
                this.CancelJob(this, e);
            }
        }

        /// <summary>
        /// Fires <see cref="DequeueJob"/> this instance's original <see cref="AppDomain"/>.
        /// </summary>
        public void FireDequeueJob(JobRecordEventArgs e)
        {
            if (this.DequeueJob != null)
            {
                this.DequeueJob(this, e);
            }
        }

        /// <summary>
        /// Fires <see cref="EnqueueScheduledJob"/> this instance's original <see cref="AppDomain"/>.
        /// </summary>
        public void FireEnqueueScheduledJob(JobRecordEventArgs e)
        {
            if (this.EnqueueScheduledJob != null)
            {
                this.EnqueueScheduledJob(this, e);
            }
        }

        /// <summary>
        /// Fires <see cref="Error"/> this instance's original <see cref="AppDomain"/>.
        /// </summary>
        public void FireError(JobErrorEventArgs e)
        {
            if (this.Error != null)
            {
                this.Error(this, e);
            }
        }

        /// <summary>
        /// Fires <see cref="FinishJob"/> this instance's original <see cref="AppDomain"/>.
        /// </summary>
        public void FireFinishJob(JobRecordEventArgs e)
        {
            if (this.FinishJob != null)
            {
                this.FinishJob(this, e);
            }
        }

        /// <summary>
        /// Fires <see cref="TimeoutJob"/> this instance's original <see cref="AppDomain"/>.
        /// </summary>
        public void FireTimeoutJob(JobRecordEventArgs e)
        {
            if (this.TimeoutJob != null)
            {
                this.TimeoutJob(this, e);
            }
        }

        #endregion
    }
}
