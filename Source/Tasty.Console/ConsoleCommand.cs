//-----------------------------------------------------------------------
// <copyright file="ConsoleCommand.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Console
{
    using System;
    using System.IO;
    using NDesk.Options;

    /// <summary>
    /// Base class for console commands.
    /// </summary>
    internal abstract class ConsoleCommand : IDisposable
    {
        private string[] args;

        /// <summary>
        /// Initializes a new instance of the ConsoleCommand class.
        /// </summary>
        /// <param name="args">The command's input arguments.</param>
        protected ConsoleCommand(string[] args)
        {
            this.args = args ?? new string[0];
        }

        /// <summary>
        /// Gets the name of the program input argument that is used to trigger this command.
        /// </summary>
        public abstract string ArgumentName { get; }

        /// <summary>
        /// Gets the command's standard error stream.
        /// </summary>
        protected virtual TextWriter StandardError
        {
            get { return System.Console.Error; }
        }

        /// <summary>
        /// Gets the command's standard input stream.
        /// </summary>
        protected virtual TextReader StandardIn
        {
            get { return System.Console.In; }
        }

        /// <summary>
        /// Gets the command's standard output stream.
        /// </summary>
        protected virtual TextWriter StandardOut
        {
            get { return System.Console.Out; }
        }

        /// <summary>
        /// Creates a new console command from the given command string and argument collection.
        /// </summary>
        /// <param name="command">The string identifying the command to create.</param>
        /// <param name="args">The arguments to pass to the command.</param>
        /// <returns>The created command.</returns>
        /// <exception cref="System.ArgumentException"></exception>
        public static ConsoleCommand Create(string command, string[] args)
        {
            switch (command.ToUpperInvariant())
            {
                case "SQL-INSTALL":
                    return new SqlInstallCommand(args);
                case "VALIDATE-OPEN-XML":
                    return new ValidateOpenXmlCommand(args);
                default:
                    throw new ArgumentException("The given command was not recognized.", "command");
            }
        }

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>A value indicating whether the command completed successfully.</returns>
        public abstract bool Execute();

        /// <summary>
        /// Writes a message indicating a bad command argument was encountered
        /// to the standard error stram.
        /// </summary>
        /// <param name="message">The message to write.</param>
        protected virtual void BadArgument(string message)
        {
            this.BadArgument(message, null);
        }

        /// <summary>
        /// Writes a message indicating a bad command argument was encountered
        /// to the standard error stram.
        /// </summary>
        /// <param name="format">The message format string to write.</param>
        /// <param name="args">Any formatting arguments to use when formatting the message.</param>
        protected virtual void BadArgument(string format, params object[] args)
        {
            this.StandardError.Write("Tasty {0}: ", this.ArgumentName);
            this.StandardError.WriteLine(format, args);
        }

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        /// <param name="disposing">A value indicating whether to dispose of managed resources.</param>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// Writes an error message.
        /// </summary>
        /// <param name="format">The message format to write.</param>
        /// <param name="args">The message arguments to write.</param>
        protected virtual void Error(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            this.StandardError.WriteLine(format, args);
            Console.ResetColor();
        }

        /// <summary>
        /// Writes a help message to the standard output stream.
        /// </summary>
        /// <param name="options">The option set to use when generating the help message.</param>
        protected virtual void Help(OptionSet options)
        {
            this.StandardOut.WriteLine("Arguments:");
            options.WriteOptionDescriptions(this.StandardOut);
        }

        /// <summary>
        /// Gets the input arguments supplied to the command.
        /// </summary>
        /// <returns>The command's supplied input arguments.</returns>
        protected string[] InputArgs()
        {
            return this.args;
        }

        /// <summary>
        /// Writes a message indicating a command argument parse error to 
        /// the standard error stream.
        /// </summary>
        /// <param name="options">The options that failed to parse.</param>
        /// <param name="ex">The parsing exception that was thrown.</param>
        protected virtual void ParseError(OptionSet options, OptionException ex)
        {
            this.StandardError.Write("Tasty {0}: ", this.ArgumentName);
            this.StandardError.WriteLine(ex.Message);
            this.StandardError.WriteLine();
            options.WriteOptionDescriptions(this.StandardError);
            return;
        }
    }
}
