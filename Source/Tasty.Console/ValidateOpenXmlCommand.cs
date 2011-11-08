//-----------------------------------------------------------------------
// <copyright file="ValidateOpenXmlCommand.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Console
{
    using System;
    using System.IO;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Validation;
    using NDesk.Options;

    /// <summary>
    /// Implements <see cref="ConsoleCommand"/> to validate Open XML documents.
    /// </summary>
    internal sealed class ValidateOpenXmlCommand : ConsoleCommand
    {
        /// <summary>
        /// Initializes a new instance of the ValidateOpenXmlCommand class.
        /// </summary>
        /// <param name="args">The command's input arguments.</param>
        public ValidateOpenXmlCommand(string[] args)
            : base(args)
        {
        }

        /// <summary>
        /// Gets the name of the program input argument that is used to trigger this command.
        /// </summary>
        public override string ArgumentName
        {
            get { return "validate-open-xml"; }
        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <returns>A value indicating whether the command completed successfully.</returns>
        public override bool Execute()
        {
            string file = null;
            int man = 0;

            var options = new OptionSet()
            {
                { "f|file=", "(required) the path to the Open XML document to validate.", v => file = v },
                { "m|man", "show this message", v => { ++man; } }
            };

            try
            {
                options.Parse(this.InputArgs());
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

            if (!string.IsNullOrEmpty(file))
            {
                try
                {
                    if (!Path.IsPathRooted(file))
                    {
                        file = Path.GetFullPath(file);
                    }
                }
                catch (ArgumentException)
                {
                    BadArgument("Please specify a valid input file path.");
                    return false;
                }

                if (File.Exists(file))
                {
                    try
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        OpenXmlValidator validator = new OpenXmlValidator();
                        int count = 0;
                        
                        foreach (ValidationErrorInfo error in validator.Validate(this.OpenPackage(file)))
                        {
                            StandardOut.WriteLine("Error {0}", ++count);
                            StandardOut.WriteLine("Description: {0}", error.Description);
                            StandardOut.WriteLine("Path: {0}", error.Path.XPath);
                            StandardOut.WriteLine("Part: {0}", error.Part.Uri);
                            StandardOut.WriteLine("-------------------------------------------");
                        }

                        Console.ResetColor();

                        if (count == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            StandardOut.WriteLine("No validation errors were found.");
                            Console.ResetColor();
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Error("{0}\n{1}", ex.Message, ex.StackTrace);
                        return false;
                    }
                    finally
                    {
                        Console.ResetColor();
                    }
                }
                else
                {
                    BadArgument("No input file was found at '{0}'.", file);
                    return false;
                }
            }
            else
            {
                BadArgument("An input file is required.");
                return false;
            }
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
            StandardOut.WriteLine("Usage: TastyConsole validate-open-xml -f:INPUT_FILE [OPTIONS]+");
            StandardOut.WriteLine("Validates Open XML documents.");
            StandardOut.WriteLine();

            base.Help(options);
        }

        /// <summary>
        /// Opens the Open XML package at the given path.
        /// </summary>
        /// <param name="path">The path to open the package at.</param>
        /// <returns>An Open XML package.</returns>
        private OpenXmlPackage OpenPackage(string path)
        {
            switch (Path.GetExtension(path).ToUpperInvariant())
            {
                case ".DOCX":
                    return WordprocessingDocument.Open(path, false);
                case ".PPTX":
                    return PresentationDocument.Open(path, false);
                default:
                    return SpreadsheetDocument.Open(path, false);
            }
        }
    }
}
