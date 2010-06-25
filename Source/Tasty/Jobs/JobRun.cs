

namespace Tasty.Jobs
{
    using System;
    using System.Threading;

    /// <summary>
    /// Represents a single, individually-threaded job run.
    /// </summary>
    internal sealed class JobRun
    {
        private Thread executionThread;

        /// <summary>
        /// Initializes a new instance of the JobRun class.
        /// </summary>
        /// <param name="jobId">The ID of the job to run.</param>
        /// <param name="job">The job to run.</param>
        public JobRun(int jobId, IJob job)
        {
            if (jobId < 1)
            {
                throw new ArgumentOutOfRangeException("jobId", "jobId must be greater than 0.");
            }

            if (job == null)
            {
                throw new ArgumentNullException("job", "job must have a value.");
            }

            this.JobId = jobId;
            this.Job = job;
        }

        /// <summary>
        /// Event fired when the job run has finished.
        /// </summary>
        public event EventHandler<JobRunEventArgs> Finished;

        /// <summary>
        /// Gets an exception that occurred during execution, if applicable.
        /// </summary>
        public Exception ExecutionException { get; private set; }

        /// <summary>
        /// Gets the date the run was finished, if applicable.
        /// </summary>
        public DateTime? FinishDate { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the run is currently in progress.
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
        /// Gets the date the job was started, if applicable.
        /// </summary>
        public DateTime? StartDate { get; private set; }

        /// <summary>
        /// Aborts the job run if it is currently in progress.
        /// </summary>
        public void Abort()
        {
            lock (this)
            {
                if (this.IsRunning)
                {
                    try
                    {
                        if (this.executionThread != null && this.executionThread.IsAlive)
                        {
                            this.executionThread.Abort();
                            this.executionThread = null;
                        }
                    }
                    catch
                    {
                    }

                    this.IsRunning = false;
                    this.FinishDate = DateTime.UtcNow;
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
                if (!this.IsRunning && this.FinishDate == null)
                {
                    this.IsRunning = true;
                    this.StartDate = DateTime.UtcNow;

                    this.executionThread = new Thread(this.RunInternal);
                    this.executionThread.Start();
                }
            }
        }

        /// <summary>
        /// Concrete job execution method.
        /// </summary>
        private void RunInternal()
        {
            try
            {
                this.Job.Execute();

                lock (this)
                {
                    this.IsRunning = false;
                    this.FinishDate = DateTime.UtcNow;

                    if (this.Finished != null)
                    {
                        this.Finished(this, new JobRunEventArgs(this.JobId));
                    }
                }
            }
            catch (Exception ex)
            {
                lock (this)
                {
                    this.ExecutionException = ex;
                    this.IsRunning = false;
                    this.FinishDate = DateTime.UtcNow;

                    if (this.Finished != null)
                    {
                        this.Finished(this, new JobRunEventArgs(this.JobId));
                    }
                }
            }
        }
    }
}
