//-----------------------------------------------------------------------
// <copyright file="GetVersion.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Build
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// Extends <see cref="Task"/> to get version information from an assembly or AssemblyInfo.cs file.
    /// </summary>
    public class GetVersion : Task
    {
        /// <summary>
        /// Gets or sets the path to the assembly DLL to pull version information from.
        /// </summary>
        public string AssemblyFile { get; set; }

        /// <summary>
        /// Gets or sets the path of the AssemblyInfo.cs file to pull version information from.
        /// </summary>
        public string AssemblyInfoFile { get; set; }

        /// <summary>
        /// Gets the version's build number.
        /// </summary>
        [Output]
        public int Build { get; private set; }

        /// <summary>
        /// Gets the version's major number.
        /// </summary>
        [Output]
        public int Major { get; private set; }

        /// <summary>
        /// Gets the version's minor number.
        /// </summary>
        [Output]
        public int Minor { get; private set; }

        /// <summary>
        /// Gets the version's revision number.
        /// </summary>
        [Output]
        public int Revision { get; private set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>True if the task executed successfully, false otherwise.</returns>
        public override bool Execute()
        {
            Version version = null;
            string error = null;

            if (!String.IsNullOrEmpty(this.AssemblyFile))
            {
                if (File.Exists(this.AssemblyFile))
                {
                    version = Assembly.ReflectionOnlyLoadFrom(this.AssemblyFile).GetName().Version;
                }
                else
                {
                    error = String.Format(CultureInfo.InvariantCulture, "The file \"{0}\" does not exist.", this.AssemblyFile);
                }
            }
            else if (!String.IsNullOrEmpty(this.AssemblyInfoFile))
            {
                if (File.Exists(this.AssemblyInfoFile))
                {
                    using (FileStream stream = File.OpenRead(this.AssemblyInfoFile))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string contents = reader.ReadToEnd();
                            Match match = Regex.Match(contents, Extensions.AssemblyVersionPattern);

                            if (match.Success)
                            {
                                string value = match.Groups[2].Value.Replace("\"", String.Empty).Trim();
                                version = new Version(value);
                            }
                        }
                    }
                }
                else
                {
                    error = String.Format(CultureInfo.InvariantCulture, "The file \"{0}\" does not exist.", this.AssemblyInfoFile);
                }
            }
            else
            {
                error = "You must specify either AssemblyFile or AssemblyInfoFile.";
            }

            if (version != null)
            {
                this.Major = version.Major;
                this.Minor = version.Minor;
                this.Build = version.Build;
                this.Revision = version.Revision;
            }

            if (!String.IsNullOrEmpty(error) && BuildEngine != null)
            {
                Log.LogError(error, null);
            }

            return version != null;
        }
    }
}
