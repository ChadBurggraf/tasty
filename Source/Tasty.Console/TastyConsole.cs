﻿//-----------------------------------------------------------------------
// <copyright file="TastyConsole.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
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
    /// The main execution handler for TastyConsole.exe.
    /// </summary>
    public static class TastyConsole
    {
        /// <summary>
        /// Main execution method.
        /// </summary>
        /// <param name="args">Input arguments.</param>
        /// <returns>The program's exit code.</returns>
        public static int Main(string[] args)
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
                return 1;
            }

            try
            {
                options.Parse(commandArgs);
            }
            catch (OptionException ex)
            {
                System.Console.Error.Write("Tasty: ");
                System.Console.Error.WriteLine(ex.Message);
                System.Console.Error.WriteLine("Try 'TastyConsole --help' for more information.");
                return 1;
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
                    return 1;
                }
            }

            return 0;
        }

        /// <summary>
        /// Writes the help message for the given option set to the given output stream.
        /// </summary>
        /// <param name="options">The option set to write the help message for.</param>
        /// <param name="output">The output stream to write the help message to.</param>
        private static void Help(OptionSet options, TextWriter output)
        {
            output.WriteLine("Usage: TastyConsole command [COMMAND_ARGUMENTS]+");
            output.WriteLine("Executes one of the available Tasty.dll command actions.");
            output.WriteLine("Use 'TastyConsole command --man' to show the manual for any command listed below.");
            output.WriteLine();
            output.WriteLine("Commands:");
            output.WriteLine("  sql-install                a SQL install script generator.");
            output.WriteLine("  validate-open-xml          validate Open XML documents.");
            options.WriteOptionDescriptions(output);
        }
    }
}
