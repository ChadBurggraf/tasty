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
    using System.Xml.Linq;
    using NDesk.Options;
    using Tasty.Configuration;
    using Tasty.Jobs;

    /// <summary>
    /// Implements <see cref="ConsoleCommand"/> to run a tasty job runner.
    /// </summary>
    internal class JobRunnerCommand : ConsoleCommand, IJobRunnerDelegate
    {
        private bool verbose, log;
        private string logPath;

        /// <summary>
        /// Initializes a new instance of the SqlInstallCommand class.
        /// </summary>
        /// <param name="args">The command's input arguments.</param>
        public JobRunnerCommand(string[] args)
            : base(args)
        {
        }

        /// <summary>
        /// Gets the name of the program input argument that is used to trigger this command.
        /// </summary>
        public override string ArgumentName
        {
            get { return "jobs"; }
        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        public override void Execute()
        {
            string config = null, log = null;
            int verbose = 0, man = 0;

            var options = new OptionSet()
            {
                { "c|config=", "(optional) the path to the configuration file to use for configuring the session.", v => config = v },
                { "v|verbose", "(optional) write session output to the console.", v => { ++verbose; } },
                { "l|log=", "(optional) the path to a file to log session output to.", v => log = v },
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
                this.Help(options);
                return;
            }

            if (!String.IsNullOrEmpty(config))
            {
                if (File.Exists(config))
                {
                    ExeConfigurationFileMap configMap = new ExeConfigurationFileMap() { ExeConfigFilename = config };

                    try
                    {
                        Configuration customConfig = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
                        TastySettings customTastySettings = customConfig.GetSection("tasty") as TastySettings;

                        if (customTastySettings == null)
                        {
                            StandardOut.WriteLine("The configuration file '{0}' does not contain a 'tasty' section. Falling back to the default configuration.\n", config);
                        }
                        else
                        {
                            Configuration processConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                            AppSettingsSection customAppSettings = (AppSettingsSection)customConfig.GetSection("appSettings");
                            
                            if (customAppSettings != null)
                            {
                                foreach (KeyValueConfigurationElement appSetting in customAppSettings.Settings)
                                {
                                    processConfig.AppSettings.Settings.Remove(appSetting.Key);
                                    processConfig.AppSettings.Settings.Add(appSetting.Key, appSetting.Value);
                                }
                            }

                            ConnectionStringsSection customConnectionStrings = (ConnectionStringsSection)customConfig.GetSection("connectionStrings");
                            
                            if (customConnectionStrings != null)
                            {
                                foreach (ConnectionStringSettings connectionString in customConnectionStrings.ConnectionStrings)
                                {
                                    processConfig.ConnectionStrings.ConnectionStrings.Remove(connectionString.Name);
                                    processConfig.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings(connectionString.Name, connectionString.ConnectionString));
                                }
                            }

                            ConfigurationSection processTastySettings = new TastySettings();
                            processTastySettings.SectionInformation.SetRawXml(customTastySettings.SectionInformation.GetRawXml());
                            processConfig.Sections.Remove("tasty");
                            processConfig.Sections.Add("tasty", processTastySettings);

                            processConfig.Save(ConfigurationSaveMode.Modified);

                            ConfigurationManager.RefreshSection("appSettings");
                            ConfigurationManager.RefreshSection("connectionStrings");
                            ConfigurationManager.RefreshSection("tasty");

                            TastySettings.Section = null;
                        }
                    }
                    catch (ConfigurationErrorsException ex)
                    {
                        BadArgument("The configuration file '{0}' has configuration errors: {1}", config, ex.Message);
                        return;
                    }
                }
                else
                {
                    BadArgument("The configuration file '{0}' does not exist.", config);
                    return;
                }
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

            this.verbose = verbose > 0;

            StandardOut.WriteLine("The tasty job runner is active.");
            StandardOut.WriteLine("The current job store is: '{0}'.", TastySettings.Section.Jobs.Store.JobStoreType); 
            StandardOut.WriteLine("Press Ctl+C to exit.\n");

            JobRunner.Instance.Start(this);

            while (true)
            {
                System.Console.ReadKey();
            }
        }

        /// <summary>
        /// Called when a job is canceled.
        /// </summary>
        /// <param name="record">The job record identifying the affected job.</param>
        public void OnCancelJob(JobRecord record)
        {
            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            Log("Canceled '{0}' ({1})", record.Name, record.Id);
            System.Console.ResetColor();
        }

        /// <summary>
        /// Called when a job is dequeued.
        /// </summary>
        /// <param name="record">The job record identifying the affected job.</param>
        public void OnDequeueJob(JobRecord record)
        {
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            Log("Dequeued '{0}' ({1})", record.Name, record.Id);
            System.Console.ResetColor();
        }

        /// <summary>
        /// Called when a scheduled job is enqueued.
        /// </summary>
        /// <param name="record">The job record identifying the affected job.</param>
        public void OnEnqueueScheduledJob(JobRecord record)
        {
            System.Console.ForegroundColor = ConsoleColor.Gray;
            Log("Enqueued '{0}' ({1}) for schedule '{2}'", record.Name, record.Id, record.ScheduleName);
            System.Console.ResetColor();
        }

        /// <summary>
        /// Called when an error occurs during the execution of the run-loop.
        /// Does not get called when a job itself experiences an error; job-specific
        /// errors are saved in the job store with their respecitve records.
        /// </summary>
        /// <param name="record">The record on which the error occurred, if applicable.</param>
        /// <param name="ex">The exception raised, if applicable.</param>
        public void OnError(JobRecord record, Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;

            if (record != null && ex != null)
            {
                string message = ExceptionXElement.Parse(record.Exception).Descendants("Message").First().Value;
                Log("An error occurred during the run loop for '{0}' ({1}). The message received was: '{2}'", record.Name, record.Id, message);
            }
            else if (record != null)
            {
                Log("An error occurred during the run loop for '{0}' ({1})", record.Name, record.Id);
            }
            else if (ex != null)
            {
                Log("An error occurred during the run loop. The message received was: '{0}'", ex.Message);
            }
            else
            {
                Log("An unspecified error occurred during the run loop");
            }

            System.Console.ResetColor();
        }

        /// <summary>
        /// Called when a job is finished.
        /// </summary>
        /// <param name="record">The job record identifying the affected job.</param>
        public void OnFinishJob(JobRecord record)
        {
            if (record.Status == JobStatus.Succeeded)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                Log("'{0}' ({1}) completed successfully", record.Name, record.Id);
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                string message = ExceptionXElement.Parse(record.Exception).Descendants("Message").First().Value;
                Log("'{0}' ({1}) failed with the message: ", record.Name, record.Id, message);
            }

            System.Console.ResetColor();
        }

        /// <summary>
        /// Called when a job is timed out.
        /// </summary>
        /// <param name="record">The job record identifying the affected job.</param>
        public void OnTimeoutJob(JobRecord record)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            Log("Timed out '{0}' ({1}) because it was taking too long to finish.", record.Name, record.Id);
            System.Console.ResetColor();
        }

        /// <summary>
        /// Writes a help message to the standard output stream.
        /// </summary>
        /// <param name="options">The option set to use when generating the help message.</param>
        protected override void Help(OptionSet options)
        {
            StandardOut.WriteLine("Usage: tasty jobs [OPTIONS]+");
            StandardOut.WriteLine("Starts a tasty job runner session and executes jobs until the process is ended.");
            StandardOut.WriteLine();

            base.Help(options);
        }

        /// <summary>
        /// Logs the given message to the standard output and/or the current logfile path.
        /// </summary>
        /// <param name="message">The message to log.</param>
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
    }
}
