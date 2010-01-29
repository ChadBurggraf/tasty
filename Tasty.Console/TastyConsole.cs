//-----------------------------------------------------------------------
// <copyright file="TastyConsole.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Console
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using NDesk.Options;

    /// <summary>
    /// The main execution handler for the tastyc console application.
    /// </summary>
    public static class TastyConsole
    {
        /// <summary>
        /// Main execution method.
        /// </summary>
        /// <param name="args">Input arguments.</param>
        public static void Main(string[] args)
        {
            string command = null;
            string[] commandArgs = new string[0];
            bool help = false;

            var options = new OptionSet()
            {
                { "h|help", "show this message and exit.", v => help = v != null }
            };

            if (args.Length > 0)
            {
                command = args[0].Trim();
                commandArgs = args.Skip(1).ToArray();
            }
            else
            {
                Help(options, System.Console.Out);
                return;
            }

            try
            {
                options.Parse(commandArgs);
            }
            catch (OptionException ex)
            {
                System.Console.Error.Write("tastyc: ");
                System.Console.Error.WriteLine(ex.Message);
                System.Console.Error.WriteLine("Try 'tastyc --help' for more information.");
                return;
            }

            if (help)
            {
                Help(options, System.Console.Out);
            }
            else
            {
                try
                {
                    ConsoleCommand.Create(command, args).Execute();
                }
                catch (ArgumentException)
                {
                    Help(options, System.Console.Out);
                }
            }
        }

        /// <summary>
        /// Writes the help message for the given option set to the given output stream.
        /// </summary>
        /// <param name="options">The option set to write the help message for.</param>
        /// <param name="output">The output stream to write the help message to.</param>
        private static void Help(OptionSet options, TextWriter output)
        {
            output.WriteLine("Usage: tastyc command [COMMAND_ARGUMENTS]+");
            output.WriteLine("Executes one of the available Tasty.dll command actions.");
            output.WriteLine();
            output.WriteLine("Commands:");
            output.WriteLine("  sql-install                a SQL install script generator.");
            options.WriteOptionDescriptions(output);
        }
    }
}
