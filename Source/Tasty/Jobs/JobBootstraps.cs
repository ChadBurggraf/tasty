//-----------------------------------------------------------------------
// <copyright file="JobBootstraps.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides bootup and teardown services for a <see cref="JobRunner"/>.
    /// Loads a secondary <see cref="AppDomain"/> by shadow-coping the target assemblies,
    /// and automatically performs safe-shudownt and restart of the <see cref="JobRunner"/>
    /// when changes are detected.
    /// </summary>
    public class JobBootstraps
    {
        #region Private Fields

        private string basePath, configurationFilePath;
        private AppDomain domain;
        private JobRunnerProxy proxy;
        private TastyFileSystemWatcher watcher;
        private Action onStopped;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the JobBootstraps class.
        /// </summary>
        public JobBootstraps()
        {
            this.AutoReload = true;
        }

        /// <summary>
        /// Initializes a new instance of the JobBootstraps class.
        /// </summary>
        /// <param name="basePath">The base path of the target directory containing the Tasty.dll and
        /// associated job assemblies to bootstrap.</param>
        /// <param name="configurationFilePath">The path to the target application's configuration file.</param>
        public JobBootstraps(string basePath, string configurationFilePath)
            : this()
        {
            if (String.IsNullOrEmpty(basePath))
            {
                throw new ArgumentNullException("basePath", "basePath must contain a value.");
            }

            if (String.IsNullOrEmpty(configurationFilePath))
            {
                throw new ArgumentNullException("configurationFilePath", "configurationFilePath must contain a value.");
            }

            this.basePath = basePath;
            this.configurationFilePath = configurationFilePath;
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

        #region Public Instance Properties

        /// <summary>
        /// Gets or sets a value indicating whether to automatically reload the <see cref="AppDomain"/>
        /// when it is unloaded due to changes in the target application's directory.
        /// </summary>
        public bool AutoReload { get; set; }

        /// <summary>
        /// Gets or sets the path to the target application's configuration file.
        /// </summary>
        public string ConfigurationFilePath { get; set; }

        /// <summary>
        /// Gets or sets the base path of the target directory containing the Tasty.dll and
        /// associated job assemblies to bootstrap.
        /// </summary>
        public string BasePath { get; set; }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Initiates a bootstrap pull-up if the target <see cref="AppDomain"/> isn't already loaded.
        /// </summary>
        public void PullUp()
        {
            lock (this)
            {
                if (this.domain == null)
                {
                    if (String.IsNullOrEmpty(this.BasePath) || !Directory.Exists(this.BasePath))
                    {
                        throw new InvalidOperationException("BasePath must be set to an existing directory location.");
                    }

                    if (String.IsNullOrEmpty(this.ConfigurationFilePath) || !File.Exists(this.ConfigurationFilePath))
                    {
                        throw new InvalidOperationException("ConfigurationFilePath must be set to an existing configuration file location.");
                    }

                    this.LoadAppDomain();
                }
            }
        }

        /// <summary>
        /// The opposite of <see cref="PullUp"/>, stops the target <see cref="AppDomain"/>, optionally waiting
        /// for all of the currently running jobs to complete.
        /// </summary>
        /// <param name="unwind">Performs a delayed shutdown, waiting for all running jobs to complate.
        /// WARNING: When true, this will cause the value of <see cref="AutoReload"/> to be set to false.</param>
        /// <param name="onStopped">Optional callback function to invoke when the job runner has been completely stopped.</param>
        public void PushDown(bool unwind, Action onStopped)
        {
            lock (this)
            {
                if (unwind)
                {
                    this.AutoReload = false;
                    this.onStopped = onStopped;
                    this.proxy.StopRunner();
                }
                else
                {
                    AppDomain.Unload(this.domain);

                    if (onStopped != null)
                    {
                        onStopped();
                    }
                }
            }
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Creates a <see cref="TastyFileSystemWatcher"/> to watch for changes in the target application's base directory.
        /// </summary>
        private void CreateWatcher()
        {
            this.watcher = new TastyFileSystemWatcher(this.BasePath);
            this.watcher.Operation += new FileSystemEventHandler(this.WatcherOperation);
            this.watcher.Mode = TastyFileSystemWatcherMode.Directory;
            this.watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Loads the target <see cref="AppDomain"/>.
        /// </summary>
        private void LoadAppDomain()
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = this.BasePath;
            setup.ShadowCopyFiles = "true";
            setup.ConfigurationFile = this.ConfigurationFilePath;

            this.domain = AppDomain.CreateDomain("Tasty Job Runner", null, setup);

            this.proxy = (JobRunnerProxy)this.domain.CreateInstanceAndUnwrap(GetType().Assembly.FullName, typeof(JobRunnerProxy).FullName);
            this.proxy.AllFinished += new EventHandler(this.ProxyAllJobsFinished);
            this.proxy.CancelJob += new EventHandler<JobRecordEventArgs>(this.ProxyCancelJob);
            this.proxy.DequeueJob += new EventHandler<JobRecordEventArgs>(this.ProxyDequeueJob);
            this.proxy.EnqueueScheduledJob += new EventHandler<JobRecordEventArgs>(this.EnqueueScheduledJob);
            this.proxy.Error += new EventHandler<JobErrorEventArgs>(this.Error);
            this.proxy.FinishJob += new EventHandler<JobRecordEventArgs>(this.FinishJob);
            this.proxy.TimeoutJob += new EventHandler<JobRecordEventArgs>(this.TimeoutJob);
        }

        /// <summary>
        /// Raises the proxy's AllJobsFinished event. 
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ProxyAllJobsFinished(object sender, EventArgs e)
        {
            lock (this)
            {
                AppDomain.Unload(this.domain);

                if (this.onStopped != null)
                {
                    this.onStopped();
                    this.onStopped = null;
                }

                if (this.AutoReload)
                {
                    this.LoadAppDomain();
                }
            }
        }

        /// <summary>
        /// Raises the proxy's CancelJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ProxyCancelJob(object sender, JobRecordEventArgs e)
        {
            if (this.CancelJob != null)
            {
                this.CancelJob(this, e);
            }
        }

        /// <summary>
        /// Raises the proxy's DequeueJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ProxyDequeueJob(object sender, JobRecordEventArgs e)
        {
            if (this.DequeueJob != null)
            {
                this.DequeueJob(this, e);
            }
        }

        /// <summary>
        /// Raises the proxy's EnqueueScheduledJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ProxyEnqueueScheduledJob(object sender, JobRecordEventArgs e)
        {
            if (this.EnqueueScheduledJob != null)
            {
                this.EnqueueScheduledJob(this, e);
            }
        }

        /// <summary>
        /// Raises the proxy's Error event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ProxyError(object sender, JobErrorEventArgs e)
        {
            if (this.Error != null)
            {
                this.Error(this, e);
            }
        }

        /// <summary>
        /// Raises the proxy's FinishJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ProxyFinishJob(object sender, JobRecordEventArgs e)
        {
            if (this.FinishJob != null)
            {
                this.FinishJob(this, e);
            }
        }

        /// <summary>
        /// Raises the proxy's TimeoutJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ProxyTimeoutJob(object sender, JobRecordEventArgs e)
        {
            if (this.TimeoutJob != null)
            {
                this.TimeoutJob(this, e);
            }
        }

        /// <summary>
        /// Raises the watcher's Operation event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void WatcherOperation(object sender, FileSystemEventArgs e)
        {
            lock (this)
            {
                this.proxy.StopRunner();
            }
        }

        #endregion
    }
}
