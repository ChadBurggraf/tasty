//-----------------------------------------------------------------------
// <copyright file="JobRunnerCommand.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Console
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security;
    using System.Text;
    using System.Threading;
    using System.Xml.Linq;
    using NDesk.Options;
    using Tasty.Configuration;
    using Tasty.Jobs;

    /// <summary>
    /// Implements <see cref="ConsoleCommand"/> to run a tasty job runner.
    /// </summary>
    internal class JobRunnerCommand : ConsoleCommand
    {
        #region Private Fields

        private bool verbose, log, autoReload;
        private string config, directory, logPath;
        private JobRunnerBootstraps bootstaps;
        private ManualResetEvent exitHandle;
        private bool disposed;

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
        public override void Execute()
        {
            string config = null, log = null, directory = null;
            int verbose = 0, man = 0;

            var options = new OptionSet()
            {
                { "d|directory=", "(required) the path to the application directory of the target Tasty.dll and job assemblies to run.", v => directory = v },
                { "c|config=", "(required) the path to a configuration file to use.", v => config = v },
                { "v|verbose", "write session output to the console.", v => { ++verbose; } },
                { "l|log=", "the path to a file to log session output to.", v => log = v },
                { "m|man", "show this message", v => { ++man; } }
            };

            try
            {
                options.Parse(this.InputArgs());
            }
            catch (OptionException ex)
            {
                ParseError(options, ex);
                return;
            }

            if (man > 0)
            {
                Help(options);
                return;
            }

            if (String.IsNullOrEmpty(directory))
            {
                Help(options);
                return;
            }
            else if (!Directory.Exists(directory))
            {
                BadArgument("There directory '{0}' does not exist.", directory);
                return;
            }
            else if (!File.Exists(Path.Combine(directory, "Tasty.dll")))
            {
                BadArgument("Tasty.dll could not be located at '{0}'.", directory);
                return;
            }

            if (String.IsNullOrEmpty(config))
            {
                Help(options);
                return;
            }
            else if (!File.Exists(config))
            {
                BadArgument("There is no configuration file at '{0}'.", config);
                return;
            }

            if (!String.IsNullOrEmpty(log))
            {
                bool canLog = false;

                try
                {
                    File.AppendAllText(log, String.Empty);
                    canLog = true;
                }
                catch (IOException)
                {
                }
                catch (SecurityException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (NotSupportedException)
                {
                }

                if (canLog)
                {
                    this.log = true;
                    this.logPath = log;
                }
                else
                {
                    BadArgument("Failed to create or open the log file at '{0}'. Please make sure you have permission to access that location.", log);
                    return;
                }
            }

            this.directory = directory;
            this.config = config;
            this.verbose = verbose > 0;
            this.autoReload = true;
            this.exitHandle = new ManualResetEvent(false);

            this.CreateAndPullUpBootstraps();

            StandardOut.WriteLine("The tasty job runner is active.");
            StandardOut.WriteLine("Press Q to safely shut down, Ctl+C to exit immediately.\n");

            new Thread(new ParameterizedThreadStart(this.WaitForInput)).Start();
            WaitHandle.WaitAll(new WaitHandle[] { this.exitHandle }, Timeout.Infinite);
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
                System.Console.ForegroundColor = ConsoleColor.DarkGray;
                this.Log("The job runner is re-starting...");
                this.CreateAndPullUpBootstraps();
                this.Log("The job runner is active.");
                System.Console.ResetColor();
            }
            else
            {
                this.Log("All jobs have finished running. Stay classy, San Diego.");
                this.exitHandle.Set();
            }
        }

        /// <summary>
        /// Raises the boostraper's CancelJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BootstrapsCancelJob(object sender, JobRecordEventArgs e)
        {
            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            this.Log("Canceled '{0}' ({1})", e.Record.Name, e.Record.Id);
            System.Console.ResetColor();
        }

        /// <summary>
        /// Raises the boostraper's ChangeDetected event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BootstrapsChangeDetected(object sender, FileSystemEventArgs e)
        {
            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            this.Log("A change was detected in '{0}'. The job runner is shutting down (it will be automatically re-started)...", e.FullPath);
            System.Console.ResetColor();
        }

        /// <summary>
        /// Raises the boostraper's DequeueJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BootstrapsDequeueJob(object sender, JobRecordEventArgs e)
        {
            System.Console.ForegroundColor = ConsoleColor.DarkCyan;
            this.Log("Dequeued '{0}' ({1})", e.Record.Name, e.Record.Id);
            System.Console.ResetColor();
        }

        /// <summary>
        /// Raises the boostraper's Error event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BootstrapsError(object sender, JobErrorEventArgs e)
        {
            System.Console.ForegroundColor = ConsoleColor.DarkRed;

            if (!String.IsNullOrEmpty(e.Record.Name) && !String.IsNullOrEmpty(e.Record.Exception))
            {
                string message = ExceptionXElement.Parse(e.Record.Exception).Descendants("Message").First().Value;
                this.Log("An error occurred during the run loop for '{0}' ({1}). The message received was: '{2}'", e.Record.Name, e.Record.Id, message);
            }
            else if (!String.IsNullOrEmpty(e.Record.Name))
            {
                this.Log("An error occurred during the run loop for '{0}' ({1})", e.Record.Name, e.Record.Id);
            }
            else if (e.Exception != null)
            {
                this.Log("An error occurred during the run loop. The message received was: '{0}'", e.Exception.Message);
            }
            else
            {
                this.Log("An unspecified error occurred during the run loop");
            }

            System.Console.ResetColor();
        }

        /// <summary>
        /// Raises the bootstrapper's ExecuteScheduledJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BootstrapsExecuteScheduledJob(object sender, JobRecordEventArgs e)
        {
            System.Console.ForegroundColor = ConsoleColor.DarkGray;
            this.Log("Started execution of '{0}' ({1}) for schedule '{2}'", e.Record.Name, e.Record.Id, e.Record.ScheduleName);
            System.Console.ResetColor();
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
                System.Console.ForegroundColor = ConsoleColor.DarkGreen;
                this.Log("'{0}' ({1}) completed successfully", e.Record.Name, e.Record.Id);
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.DarkRed;
                string message = ExceptionXElement.Parse(e.Record.Exception).Descendants("Message").First().Value;
                this.Log("'{0}' ({1}) failed with the message: {2}", e.Record.Name, e.Record.Id, message);
            }

            System.Console.ResetColor();
        }

        /// <summary>
        /// Raises the boostraper's TimeoutJob event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BootstrapsTimeoutJob(object sender, JobRecordEventArgs e)
        {
            System.Console.ForegroundColor = ConsoleColor.DarkRed;
            this.Log("Timed out '{0}' ({1}) because it was taking too long to finish.", e.Record.Name, e.Record.Id);
            System.Console.ResetColor();
        }

        /// <summary>
        /// Creates this instance's <see cref="JobRunnerBootstraps"/> object, sets up the events and executes the pull-up.
        /// </summary>
        private void CreateAndPullUpBootstraps()
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
        }

        /// <summary>
        /// Logs the given message to the standard output and/or the current logfile path.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="args">The formatting arguments to use when formatting the message.</param>
        private void Log(string message, params object[] args)
        {
            string logMessage = String.Format(CultureInfo.InvariantCulture, "{0:s}\n", DateTime.Now);
            logMessage += String.Format(CultureInfo.CurrentCulture, message, args) + "\n\n";
            
            if (this.log)
            {
                File.AppendAllText(this.logPath, logMessage, Encoding.UTF8);
            }

            if (this.verbose)
            {
                StandardOut.Write(logMessage);
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
                ConsoleKeyInfo info = System.Console.ReadKey();

                if ("q".Equals(info.Key.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    System.Console.CursorLeft = 0;

                    System.Console.ForegroundColor = ConsoleColor.DarkYellow;
                    this.Log("The tasty job runner is sutting down...");
                    System.Console.ResetColor();

                    this.autoReload = false;
                    this.bootstaps.PushDown(true);

                    break;
                }
            }
        }

        #endregion
    }
}
