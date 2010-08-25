//-----------------------------------------------------------------------
// <copyright file="JobService.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.JobService
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.ServiceProcess;

    /// <summary>
    /// <see cref="ServiceBase"/> implementation for the Tasty Job Service.
    /// </summary>
    public partial class Service : ServiceBase
    {
        #region Private Fields

        private ProcessTuple[] processes = new ProcessTuple[0];
        private bool isRunning;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the JobService class.
        /// </summary>
        public Service()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Protected Instance Methods

        /// <summary>
        /// Runs when a Continue command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service resumes normal functioning after being paused.
        /// </summary>
        protected override void OnContinue()
        {
            this.isRunning = true;
            this.StopAllProcesses(false);
            this.InitializeProcessTuples();
            this.StartAllProcesses();
        }

        /// <summary>
        /// Executes when a Pause command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service pauses.
        /// </summary>
        protected override void OnPause()
        {
            this.isRunning = false;
            this.StopAllProcesses(true);
        }

        /// <summary>
        /// Executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Input arguments.</param>
        protected override void OnStart(string[] args)
        {
            this.isRunning = true;
            this.StopAllProcesses(false);
            this.InitializeProcessTuples();
            this.StartAllProcesses();
        }

        /// <summary>
        /// Executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            this.isRunning = false;
            this.StopAllProcesses(true);
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Logs a message to the system event log.
        /// </summary>
        /// <param name="isError">A value indicating whether the message is an error.</param>
        /// <param name="format">The message format to log.</param>
        /// <param name="args">Any format arguments to use when formatting the message.</param>
        private static void LogMessage(bool isError, string format, params string[] args)
        {
            const string Source = "Tasty Job Service";

            if (!EventLog.SourceExists(Source))
            {
                EventLog.CreateEventSource(Source, "Application");
            }

            string message = String.Format(CultureInfo.InvariantCulture, format, args);
            EventLog.WriteEntry(Source, message, isError ? EventLogEntryType.Error : EventLogEntryType.Information);
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Initializes this instance's <see cref="ProcessTuple"/> collection.
        /// </summary>
        private void InitializeProcessTuples()
        {
            this.processes = new ProcessTuple[TastyJobServiceSettings.Section.Applications.Count];
            string exePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "TastyConsole.exe");

            for (int i = 0; i < this.processes.Length; i++)
            {
                var appElement = TastyJobServiceSettings.Section.Applications[i];

                this.processes[i] = new ProcessTuple()
                {
                    Application = appElement,
                    StartInfo = new ProcessStartInfo()
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        FileName = exePath,
                        Arguments = String.Format(
                            CultureInfo.InvariantCulture,
                            @"jobs -d ""{0}"" -c ""{1}"" -l",
                            appElement.Directory,
                            appElement.CfgFile
                        )
                    }
                };
            }
        }

        /// <summary>
        /// Raises a process' Exited event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ProcessExited(object sender, EventArgs e)
        {
            Process process = sender as Process;

            ProcessTuple tuple = this.processes.Where(t => t.Process == process).FirstOrDefault();

            tuple.Process.Dispose();
            tuple.Process = null;

            LogMessage(false, "A Tasty Jobs process has exited for application \"{0}\".", tuple.Application.Name);

            if (this.isRunning)
            {
                this.StartProcess(tuple);
            }
        }

        /// <summary>
        /// Starts all process.
        /// </summary>
        private void StartAllProcesses()
        {
            foreach (ProcessTuple tuple in this.processes)
            {
                this.StartProcess(tuple);
            }
        }

        /// <summary>
        /// Starts the process for the given tuple.
        /// </summary>
        /// <param name="tuple">The tuple to start the process for.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to continue execution no matter what.")]
        private void StartProcess(ProcessTuple tuple)
        {
            try
            {
                if (tuple.Process != null)
                {
                    tuple.Process.Kill();
                    tuple.Process.Dispose();
                }

                tuple.Process = new Process() { StartInfo = tuple.StartInfo, EnableRaisingEvents = true };
                tuple.Process.Exited += new EventHandler(this.ProcessExited);
                tuple.Process.Start();

                LogMessage(false, "A Tasty Jobs process was started for application \"{0}\".", tuple.Application.Name);
            }
            catch (Exception ex)
            {
                LogMessage(true, "An error occurred starting a Tasty Jobs process ({0}): {1}\n\nStack trace:\n{2}", tuple.Application.Name, ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Stops all processes.
        /// </summary>
        /// <param name="safely">A value indicating whether to issue safe-quit commands.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to continue execution no matter what.")]
        private void StopAllProcesses(bool safely)
        {
            foreach (ProcessTuple tuple in this.processes)
            {
                if (tuple.Process != null)
                {
                    try
                    {
                        if (safely)
                        {
                            tuple.Process.StandardInput.WriteLine("q");
                            LogMessage(false, "A Tasty Jobs process was issued a safe-quit command for application \"{0}\".", tuple.Application.Name);
                        }
                        else
                        {
                            try
                            {
                                tuple.Process.Kill();
                                tuple.Process.Dispose();
                                tuple.Process = null;
                            }
                            catch (Exception ex)
                            {
                                LogMessage(true, "An error occurred when force-quitting a Tasty Jobs process ({0}): {1}\n\nStack trace:\n{2}", tuple.Application.Name, ex.Message, ex.StackTrace);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage(true, "An error occurred stopping a Tasty Jobs process ({0}): {1}\n\nStack trace:\n{2}", tuple.Application.Name, ex.Message, ex.StackTrace);
                    }
                }
            }
        }

        #endregion

        #region ProcessTuple Class

        /// <summary>
        /// Represents a configured job service application and its associated process.
        /// </summary>
        private class ProcessTuple
        {
            /// <summary>
            /// Gets or sets the tuple's <see cref="ApplicationElement"/>.
            /// </summary>
            public ApplicationElement Application { get; set; }

            /// <summary>
            /// Gets or sets the tuple's <see cref="Process"/>.
            /// </summary>
            public Process Process { get; set; }

            /// <summary>
            /// Gets or sets the tuple's <see cref="ProcessStartInfo"/>.
            /// </summary>
            public ProcessStartInfo StartInfo { get; set; }
        }

        #endregion
    }
}
