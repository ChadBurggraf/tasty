//-----------------------------------------------------------------------
// <copyright file="JobRunnerCommand.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Console
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Xml.Linq;
    using log4net;
    using log4net.Appender;
    using log4net.Core;
    using log4net.Layout;
    using log4net.Repository.Hierarchy;
    using NDesk.Options;
    using Tasty.Configuration;
    using Tasty.Jobs;

    /// <summary>
    /// Implements <see cref="ConsoleCommand"/> to run a tasty job runner.
    /// </summary>
    internal class JobRunnerCommand : ConsoleCommand
    {
        #region Private Fields

        private const string PathQuotesExp = @"^[""']?([^""']*)[""']?$";

        private object bootstrapsLocker = new object();
        private bool autoReload, disposed, executeSuccess;
        private string config, directory;
        private int pullUpFailCount;
        private JobRunnerBootstraps bootstaps;
        private ManualResetEvent exitHandle;
        private Thread inputThread;
        private ILog logger;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the JobRunnerCommand class.
        /// </summary>
        /// <param name="args">The command's input arguments.</param>
        public JobRunnerCommand(string[] args)
            : base(args)
        {
        }

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets the name of the program input argument that is used to trigger this command.
        /// </summary>
        public override string ArgumentName
        {
            get { return "jobs"; }
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <returns>A value indicating whether the command completed successfully.</returns>
        public override bool Execute()
        {
            string config = null, directory = null;
            int enableLogging = 0, verbose = 0, man = 0;

            var options = new OptionSet()
            {
                { "d|directory=", "(required) the path to the application directory of the target Tasty.dll and job assemblies to run.", v => directory = v },
                { "c|config=", "(required) the path to a configuration file to use.", v => config = v },
                { "v|verbose", "write session output to the console.", v => { ++verbose; } },
                { "l|log", "enable logging to a file.", v => { ++enableLogging; } },
                { "m|man", "show this message", v => { ++man; } }
            };

            try
            {
                options.Parse(this.InputArgs());

                directory = Regex.Replace(directory, PathQuotesExp, "$1");
                config = Regex.Replace(config, PathQuotesExp, "$1");
            }
            catch (OptionException ex)
            {
                ParseError(options, ex);
                return false;
            }

            if (man > 0)
            {
                this.Help(options);
                return false;
            }

            if (String.IsNullOrEmpty(directory))
            {
                this.Help(options);
                return false;
            }
            else if (!Directory.Exists(directory))
            {
                BadArgument("The directory '{0}' does not exist.", directory);
                return false;
            }
            else if (!File.Exists(Path.Combine(directory, "Tasty.dll")))
            {
                BadArgument("Tasty.dll could not be located at '{0}'.", directory);
                return false;
            }

            if (String.IsNullOrEmpty(config))
            {
                this.Help(options);
                return false;
            }
            else if (!File.Exists(config))
            {
                BadArgument("There is no configuration file at '{0}'.", config);
                return false;
            }

            this.directory = directory;
            this.config = config;
            this.autoReload = true;
            this.exitHandle = new ManualResetEvent(false);
            this.logger = this.ConfigureAndGetLogger(verbose > 0, enableLogging > 0);

            this.CreateAndPullUpBootstraps();

            if (this.bootstaps != null && this.bootstaps.IsLoaded)
            {
                this.executeSuccess = true;

                this.inputThread = new Thread(new ParameterizedThreadStart(this.WaitForInput));
                this.inputThread.Start();

                WaitHandle.WaitAll(new WaitHandle[] { this.exitHandle }, Timeout.Infinite);
            }

            return this.executeSuccess;
        }

        #endregion

        #region Protected Instance Methods

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        /// <param name="disposing">A value indicating whether to dispose of managed resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.exitHandle != null)
                    {
                        this.exitHandle.Close();
                        this.exitHandle = null;
                    }

                    if (this.bootstaps != null)
                    {
                        this.bootstaps.Dispose();
                        this.bootstaps = null;
                    }
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Writes a help message to the standard output stream.
        /// </summary>
        /// <param name="options">The option set to use when generating the help message.</param>
        protected override void Help(OptionSet options)
        {
            StandardOut.WriteLine("Usage: tasty jobs -d:APPLICATION_DIRECTORY -c:CONFIG_PATH [OPTIONS]+");
            StandardOut.WriteLine("Starts a tasty job runner session and executes jobs until the process is ended. You must provide an application directory that contains a copy of Tasty.dll, along with your job assemblies and a configuration file for the job runner to use.");
            StandardOut.WriteLine();

            base.Help(options);
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Raises the boostraper's AllFinished event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BootstrapsAllFinished(object sender, EventArgs e)
        {
            if (this.autoReload)
            {
                this.logger.Info("The job runner is restarting.");
                this.CreateAndPullUpBootstraps();
            }
            else
            {
                this.logger.Info("All jobs have finished running. Stay classy, San Diego.");
                this.Quit();
            }
        }

        /// <summary>
        /// Raises the boostraper's CancelJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BootstrapsCancelJob(object sender, JobRecordEventArgs e)
        {
            this.logger.InfoFormat("Canceled '{0}' ({1})", e.Record.Name, e.Record.Id);
        }

        /// <summary>
        /// Raises the boostraper's ChangeDetected event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BootstrapsChangeDetected(object sender, FileSystemEventArgs e)
        {
            this.logger.InfoFormat("A change was detected in '{0}'. The job runner is shutting down (it will be automatically re-started).", e.FullPath);
        }

        /// <summary>
        /// Raises the boostraper's DequeueJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BootstrapsDequeueJob(object sender, JobRecordEventArgs e)
        {
            this.logger.InfoFormat("Dequeued '{0}' ({1}).", e.Record.Name, e.Record.Id);
        }

        /// <summary>
        /// Raises the boostraper's Error event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BootstrapsError(object sender, JobErrorEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Record.Name) && !String.IsNullOrEmpty(e.Record.Exception))
            {
                string message = ExceptionXElement.Parse(e.Record.Exception).Descendants("Message").First().Value;
                this.logger.ErrorFormat("An error occurred during the run loop for '{0}' ({1}). The message received was: '{2}'", e.Record.Name, e.Record.Id, message);
            }
            else if (!String.IsNullOrEmpty(e.Record.Name))
            {
                this.logger.ErrorFormat("An error occurred during the run loop for '{0}' ({1}).", e.Record.Name, e.Record.Id);
            }
            else if (e.Exception != null)
            {
                this.logger.Error("An error occurred during the run loop.", e.Exception);
            }
            else
            {
                this.logger.Error("An unspecified error occurred during the run loop.");
            }
        }

        /// <summary>
        /// Raises the bootstrapper's ExecuteScheduledJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BootstrapsExecuteScheduledJob(object sender, JobRecordEventArgs e)
        {
            this.logger.InfoFormat("Started execution of '{0}' ({1}) for schedule '{2}'.", e.Record.Name, e.Record.Id, e.Record.ScheduleName);
        }

        /// <summary>
        /// Raises the boostraper's FinishJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BootstrapsFinishJob(object sender, JobRecordEventArgs e)
        {
            if (e.Record.Status == JobStatus.Succeeded)
            {
                this.logger.InfoFormat("'{0}' ({1}) completed successfully.", e.Record.Name, e.Record.Id);
            }
            else
            {
                string message = ExceptionXElement.Parse(e.Record.Exception).Descendants("Message").First().Value;
                this.logger.ErrorFormat("'{0}' ({1}) failed with the message: {2}.", e.Record.Name, e.Record.Id, message);
            }
        }

        /// <summary>
        /// Raises the boostraper's TimeoutJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BootstrapsTimeoutJob(object sender, JobRecordEventArgs e)
        {
            this.logger.ErrorFormat("Timed out '{0}' ({1}) because it was taking too long to finish.", e.Record.Name, e.Record.Id);
        }

        /// <summary>
        /// Configures and returns the logger instance to use when logging.
        /// TODO: Extract this out so we can easily log other <see cref="ConsoleCommand"/>s.
        /// </summary>
        /// <param name="outputToConsole">A value indicating whether to output to the console.</param>
        /// <param name="outputToFile">A value indicating whether to output to a log file.</param>
        /// <returns>The configured logger instance.</returns>
        private ILog ConfigureAndGetLogger(bool outputToConsole, bool outputToFile)
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout layout = new PatternLayout();
            layout.ConversionPattern = "%d [%t] %-5p - %m%n";
            layout.ActivateOptions();

            if (outputToConsole)
            {
                ColoredConsoleAppender console = new ColoredConsoleAppender();
                console.Layout = layout;

                console.AddMapping(new ColoredConsoleAppender.LevelColors()
                {
                    ForeColor = ColoredConsoleAppender.Colors.Red,
                    Level = Level.Fatal
                });

                console.AddMapping(new ColoredConsoleAppender.LevelColors()
                {
                    ForeColor = ColoredConsoleAppender.Colors.Red,
                    Level = Level.Error
                });

                console.AddMapping(new ColoredConsoleAppender.LevelColors()
                {
                    ForeColor = ColoredConsoleAppender.Colors.White,
                    Level = Level.Info
                });

                console.ActivateOptions();
                hierarchy.Root.AddAppender(console);
            }

            if (outputToFile)
            {
                RollingFileAppender roller = new RollingFileAppender();
                roller.Layout = layout;
                roller.AppendToFile = true;
                roller.RollingStyle = RollingFileAppender.RollingMode.Size;
                roller.MaxSizeRollBackups = 10;
                roller.MaximumFileSize = "1MB";
                roller.StaticLogFileName = true;
                roller.File = Path.Combine(this.directory, "tasty-jobs-log.txt");
                roller.ActivateOptions();
                hierarchy.Root.AddAppender(roller);
            }

            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;

            return LogManager.GetLogger("Jobs");
        }

        /// <summary>
        /// Creates this instance's <see cref="JobRunnerBootstraps"/> object, sets up the events and executes the pull-up.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to retry the pull-up operation no matter the reason for failure.")]
        private void CreateAndPullUpBootstraps()
        {
            lock (this.bootstrapsLocker)
            {
                if (this.bootstaps != null)
                {
                    this.bootstaps.Dispose();
                }

                try
                {
                    this.bootstaps = new JobRunnerBootstraps(this.directory, this.config);
                    this.bootstaps.AllFinished += new EventHandler(this.BootstrapsAllFinished);
                    this.bootstaps.CancelJob += new EventHandler<JobRecordEventArgs>(this.BootstrapsCancelJob);
                    this.bootstaps.ChangeDetected += new EventHandler<FileSystemEventArgs>(this.BootstrapsChangeDetected);
                    this.bootstaps.DequeueJob += new EventHandler<JobRecordEventArgs>(this.BootstrapsDequeueJob);
                    this.bootstaps.Error += new EventHandler<JobErrorEventArgs>(this.BootstrapsError);
                    this.bootstaps.ExecuteScheduledJob += new EventHandler<JobRecordEventArgs>(this.BootstrapsExecuteScheduledJob);
                    this.bootstaps.FinishJob += new EventHandler<JobRecordEventArgs>(this.BootstrapsFinishJob);
                    this.bootstaps.TimeoutJob += new EventHandler<JobRecordEventArgs>(this.BootstrapsTimeoutJob);
                    this.bootstaps.PullUp();

                    this.pullUpFailCount = 0;
                    StandardOut.WriteLine("The tasty job runner is active.\nPress Q+Enter to safely shut down, Ctl+C to exit immediately.\n");
                }
                catch (Exception ex)
                {
                    this.pullUpFailCount++;
                    this.TimeoutAndRetryPullUp(ex.Message);
                }
            }
        }

        /// <summary>
        /// Quits the job runner and signals that this process can exit.
        /// </summary>
        private void Quit()
        {
            this.inputThread.Abort();
            this.exitHandle.Set();
        }

        /// <summary>
        /// Times out the current thread because of the given error message and retries
        /// <see cref="CreateAndPullUpBootstraps()"/> after the timeout is complete.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        private void TimeoutAndRetryPullUp(string message)
        {
            if (this.pullUpFailCount < 10)
            {
                this.logger.ErrorFormat("Failed to bootstrap a job runner at the destination with the message: {0}\nTrying again in 10 seconds.", message);
                
                Thread.Sleep(10000);
                this.CreateAndPullUpBootstraps();
            }
            else
            {
                this.logger.Fatal("Failed to bootstrap a job runner at the destination application 10 times. I'm giving up.");
                
                this.executeSuccess = false;
                this.Quit();
            }
        }

        /// <summary>
        /// <see cref="ThreadStart"/> delegate used to wait for console input without blocking.
        /// </summary>
        /// <param name="obj">The <see cref="ThreadStart"/> state object.</param>
        private void WaitForInput(object obj)
        {
            while (true)
            {
                bool isLoaded = false;

                lock (this.bootstrapsLocker)
                {
                    isLoaded = this.bootstaps != null && this.bootstaps.IsLoaded;
                }

                if (isLoaded)
                {
                    byte[] buffer = new byte[1];

                    System.Console.OpenStandardInput().BeginRead(
                        buffer,
                        0,
                        buffer.Length,
                        (IAsyncResult result) =>
                        {
                            string input = Encoding.ASCII.GetString(buffer).Trim();

                            if ("q".Equals(input, StringComparison.OrdinalIgnoreCase))
                            {
                                this.logger.Info("The tasty job runner is sutting down.");
                                this.autoReload = false;
                                this.bootstaps.PushDown(true);
                            }
                        },
                        null);
                }

                Thread.Sleep(250);
            }
        }

        #endregion
    }
}
