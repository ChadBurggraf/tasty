﻿//-----------------------------------------------------------------------
// <copyright file="JobRunnerBootstraps.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Policy;

    /// <summary>
    /// Provides bootup and teardown services for a <see cref="JobRunner"/>.
    /// Loads a secondary <see cref="AppDomain"/> by shadow-coping the target assemblies,
    /// and automatically performs safe-shutdown and restart of the <see cref="JobRunner"/>
    /// when changes are detected.
    /// </summary>
    public class JobRunnerBootstraps : IDisposable
    {
        #region Private Fields

        private bool disposed;
        private AppDomain domain;
        private JobRunnerProxy proxy;
        private JobRunnerEventSink eventSink;
        private TastyFileSystemWatcher directoryWatcher, configWatcher;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the JobRunnerBootstraps class.
        /// </summary>
        public JobRunnerBootstraps()
        {
        }

        /// <summary>
        /// Initializes a new instance of the JobRunnerBootstraps class.
        /// </summary>
        /// <param name="basePath">The base path of the target directory containing the Tasty.dll and
        /// associated job assemblies to bootstrap.</param>
        /// <param name="configurationFilePath">The path to the target application's configuration file.</param>
        public JobRunnerBootstraps(string basePath, string configurationFilePath)
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

            this.BasePath = basePath;
            this.ConfigurationFilePath = configurationFilePath;
        }

        #endregion

        #region Events

        /// <summary>
        /// Event raised when the job runner is shutting down and all running jobs have finished executing.
        /// </summary>
        public event EventHandler AllFinished;

        /// <summary>
        /// Event raised when a job has been canceled and and its run terminated.
        /// </summary>
        public event EventHandler<JobRecordEventArgs> CancelJob;

        /// <summary>
        /// Event raised when the target application directory or configuration file
        /// is changed and the job runner is being shut down.
        /// </summary>
        public event EventHandler<FileSystemEventArgs> ChangeDetected;

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
        /// Event raised when a sceduled job loaded for execution.
        /// </summary>
        public event EventHandler<JobRecordEventArgs> ExecuteScheduledJob;

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
        /// Gets or sets the path to the target application's configuration file.
        /// </summary>
        public string ConfigurationFilePath { get; set; }

        /// <summary>
        /// Gets or sets the base path of the target directory containing the Tasty.dll and
        /// associated job assemblies to bootstrap.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Gets a value indicating whether the target application has been successfully loaded.
        /// </summary>
        public bool IsLoaded { get; private set; }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

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

                    this.LoadAndStartAppDomain();
                    this.IsLoaded = true;
                }
            }
        }

        /// <summary>
        /// The opposite of <see cref="PullUp"/>, stops the target <see cref="AppDomain"/>, optionally waiting
        /// for all of the currently running jobs to complete.
        /// </summary>
        /// <param name="unwind">Performs a delayed shutdown, waiting for all running jobs to complate.</param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Meant as a verb, not a noun.")]
        public void PushDown(bool unwind)
        {
            lock (this)
            {
                if (this.IsLoaded)
                {
                    this.directoryWatcher.Dispose();
                    this.directoryWatcher = null;

                    this.configWatcher.Dispose();
                    this.configWatcher = null;

                    if (unwind)
                    {
                        this.proxy.StopRunner();
                    }
                    else
                    {
                        this.IsLoaded = false;
                        AppDomain.Unload(this.domain);
                    }
                }
            }
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        /// <param name="disposing">A value indicating whether to dispose of managed resources.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.configWatcher != null)
                    {
                        this.configWatcher.Dispose();
                        this.configWatcher = null;
                    }

                    if (this.directoryWatcher != null)
                    {
                        this.directoryWatcher.Dispose();
                        this.directoryWatcher = null;
                    }
                }

                this.disposed = true;
                this.IsLoaded = false;
            }
        }

        /// <summary>
        /// Loads the <see cref="AppDomain"/> to run the <see cref="JobRunner"/> in and starts the runner.
        /// </summary>
        private void LoadAndStartAppDomain()
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = this.BasePath;
            setup.ShadowCopyFiles = "true";
            setup.ConfigurationFile = this.ConfigurationFilePath;

            this.domain = AppDomain.CreateDomain("Tasty Job Runner", AppDomain.CurrentDomain.Evidence, setup);
            this.proxy = (JobRunnerProxy)this.domain.CreateInstanceAndUnwrap("Tasty", "Tasty.Jobs.JobRunnerProxy");

            this.eventSink = new JobRunnerEventSink();
            this.eventSink.AllFinished += new EventHandler(this.ProxyAllFinished);
            this.eventSink.CancelJob += new EventHandler<JobRecordEventArgs>(this.ProxyCancelJob);
            this.eventSink.DequeueJob += new EventHandler<JobRecordEventArgs>(this.ProxyDequeueJob);
            this.eventSink.Error += new EventHandler<JobErrorEventArgs>(this.ProxyError);
            this.eventSink.ExecuteScheduledJob += new EventHandler<JobRecordEventArgs>(this.ProxyExecuteScheduledJob);
            this.eventSink.FinishJob += new EventHandler<JobRecordEventArgs>(this.ProxyFinishJob);
            this.eventSink.TimeoutJob += new EventHandler<JobRecordEventArgs>(this.ProxyTimeoutJob);
            this.proxy.EventSink = this.eventSink;

            this.directoryWatcher = new TastyFileSystemWatcher(this.BasePath);
            this.directoryWatcher.Operation += new FileSystemEventHandler(this.WatcherOperation);
            this.directoryWatcher.Mode = TastyFileSystemWatcherMode.Directory;
            this.directoryWatcher.Filter = "*.dll";
            this.directoryWatcher.EnableRaisingEvents = true;

            this.configWatcher = new TastyFileSystemWatcher(Path.GetDirectoryName(this.ConfigurationFilePath));
            this.configWatcher.Operation += new FileSystemEventHandler(this.WatcherOperation);
            this.configWatcher.Mode = TastyFileSystemWatcherMode.IndividualFiles;
            this.configWatcher.Filter = Path.GetFileName(this.ConfigurationFilePath);
            this.configWatcher.EnableRaisingEvents = true;

            this.proxy.StartRunner();
        }

        /// <summary>
        /// Raises the proxy's AllFinished event. 
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ProxyAllFinished(object sender, EventArgs e)
        {
            lock (this)
            {
                this.IsLoaded = false;

                this.proxy = null;
                this.eventSink = null;

                this.RaiseEvent(this.AllFinished, e);
                
                AppDomain.Unload(this.domain);
            }
        }

        /// <summary>
        /// Raises the proxy's CancelJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ProxyCancelJob(object sender, JobRecordEventArgs e)
        {
            this.RaiseEvent(this.CancelJob, e);
        }

        /// <summary>
        /// Raises the proxy's DequeueJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ProxyDequeueJob(object sender, JobRecordEventArgs e)
        {
            this.RaiseEvent(this.DequeueJob, e);
        }

        /// <summary>
        /// Raises the proxy's Error event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ProxyError(object sender, JobErrorEventArgs e)
        {
            this.RaiseEvent(this.Error, e);
        }

        /// <summary>
        /// Raises the proxy's ExecuteScheduledJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ProxyExecuteScheduledJob(object sender, JobRecordEventArgs e)
        {
            this.RaiseEvent(this.ExecuteScheduledJob, e);
        }

        /// <summary>
        /// Raises the proxy's FinishJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ProxyFinishJob(object sender, JobRecordEventArgs e)
        {
            this.RaiseEvent(this.FinishJob, e);
        }

        /// <summary>
        /// Raises the proxy's TimeoutJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ProxyTimeoutJob(object sender, JobRecordEventArgs e)
        {
            this.RaiseEvent(this.TimeoutJob, e);
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
                this.RaiseEvent(this.ChangeDetected, e);
                this.PushDown(true);
            }
        }

        #endregion
    }
}
