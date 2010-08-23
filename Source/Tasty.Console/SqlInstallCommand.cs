//-----------------------------------------------------------------------
// <copyright file="SqlInstallCommand.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Console
{
    using System;
    using System.IO;
    using System.Linq;
    using NDesk.Options;
    using Tasty.Build;

    /// <summary>
    /// Implements <see cref="ConsoleCommand"/> to generate SQL install scripts.
    /// </summary>
    internal class SqlInstallCommand : ConsoleCommand
    {
        /// <summary>
        /// Initializes a new instance of the SqlInstallCommand class.
        /// </summary>
        /// <param name="args">The command's input arguments.</param>
        public SqlInstallCommand(string[] args)
            : base(args)
        {
        }

        /// <summary>
        /// Gets the name of the program input argument that is used to trigger this command.
        /// </summary>
        public override string ArgumentName
        {
            get { return "sql-install"; }
        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        public override void Execute()
        {
            string from = null, to = null, dir = null, output = null;
            int verbose = 0, man = 0;

            var options = new OptionSet()
            {
                { "d|dir=", "(required) the path to the directory to search for SQL scripts in.", v => dir = v },
                { "o|out=", "(required) the path of the file to create or overwrite with the generated script.", v => output = v },
                { "f|from=", "the lower-bound version number to restrict the generated script to (e.g., 0.3 or 2.4.5), exclusive.", v => from = v },
                { "t|to=", "the upper-bound version number to restrict the generated script to (e.g., 1.0 or 1.2.10.3), inclusive.", v => to = v },
                { "v|verbose", "write generation progress to the console.", v => { ++verbose; } },
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

            if (String.IsNullOrEmpty(dir) || String.IsNullOrEmpty(output))
            {
                Help(options);
                return;
            }

            bool badVersions = false;

            Version fromVersion = null, toVersion = null;

            try
            {
                if (!String.IsNullOrEmpty(from))
                {
                    fromVersion = new Version(from);
                }

                if (!String.IsNullOrEmpty(to))
                {
                    toVersion = new Version(to);
                }
            }
            catch (ArgumentException)
            {
                badVersions = true;
            }
            catch (FormatException)
            {
                badVersions = true;
            }
            catch (OverflowException)
            {
                badVersions = true;
            }

            if (badVersions)
            {
                BadArgument("Please provide a valid version number.");
                return;
            }

            if (!Directory.Exists(dir))
            {
                BadArgument("The directory '{0}' does not exist.", dir);
                return;
            }

            SchemaUpgradeService.GenerateInstallScript(
                fromVersion, 
                toVersion, 
                dir, 
                output, 
                delegate(Version version, string path)
                {
                    if (verbose > 0) 
                    {
                        StandardOut.WriteLine("Added script {0} for version {1}.", path, version);
                    }
                });
        }

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        /// <param name="disposing">A value indicating whether to dispose of managed resources.</param>
        protected override void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Writes a help message to the standard output stream.
        /// </summary>
        /// <param name="options">The option set to use when generating the help message.</param>
        protected override void Help(OptionSet options)
        {
            StandardOut.WriteLine("Usage: tasty sql-install -d:INPUT_DIRECTORY -o:OUTPUT_FILE [OPTIONS]+");
            StandardOut.WriteLine("Generates a SQL install script by concatenating versioned script files in the given directory.");
            StandardOut.WriteLine();

            base.Help(options);
        }
    }
}
