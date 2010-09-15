//-----------------------------------------------------------------------
// <copyright file="SetVersion.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Build
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// Implements <see cref="Task"/> to set version information in AssemblyInfo files.
    /// </summary>
    public class SetVersion : Task
    {
        /// <summary>
        /// Initializes a new instance of the SetVersion class.
        /// </summary>
        public SetVersion()
        {
            this.SetAssemblyFileVersion = true;
            this.SetAssemblyVersion = true;
        }

        /// <summary>
        /// Gets or sets the file set to set version information in.
        /// </summary>
        [Required]
        public ITaskItem[] Files { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to set the AssemblyFileVersion attribute.
        /// </summary>
        public bool SetAssemblyFileVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to set the AssemblyVersion attribute.
        /// </summary>
        public bool SetAssemblyVersion { get; set; }

        /// <summary>
        /// Gets or sets the version string to set.
        /// </summary>
        [Required]
        public string Version { get; set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>True if the task executed successfully, false otherwise.</returns>
        public override bool Execute()
        {
            string error = null;

            foreach (string file in this.Files.Select(ti => ti.ItemSpec))
            {
                if (File.Exists(file))
                {
                    string message = null;
                    string contents = File.ReadAllText(file);

                    if (this.SetAssemblyFileVersion)
                    {
                        if (Regex.IsMatch(contents, Extensions.AssemblyFileVersionPattern))
                        {
                            contents = Regex.Replace(contents, Extensions.AssemblyFileVersionPattern, String.Concat("$1\"", this.Version, "\"$3"));
                            message = String.Format(CultureInfo.InvariantCulture, "AssemblyFileVersion attribute set to \"{0}\" in file \"{1}\".", this.Version, file);
                        }
                        else
                        {
                            message = String.Format(CultureInfo.InvariantCulture, "No AssemblyFileVersion attribute found in file \"{0}\".", file);
                        }

                        if (BuildEngine != null)
                        {
                            Log.LogMessage(message, null);
                        }

                        message = null;
                    }

                    if (this.SetAssemblyVersion)
                    {
                        if (Regex.IsMatch(contents, Extensions.AssemblyVersionPattern))
                        {
                            contents = Regex.Replace(contents, Extensions.AssemblyVersionPattern, String.Concat("$1\"", this.Version, "\"$3"));
                            message = String.Format(CultureInfo.InvariantCulture, "AssemblyVersion attribute set to \"{0}\" in file \"{1}\".", this.Version, file);
                        }
                        else
                        {
                            message = String.Format(CultureInfo.InvariantCulture, "No AssemblyVersion attribute found in file \"{0}\".", file);
                        }

                        if (BuildEngine != null)
                        {
                            Log.LogMessage(message, null);
                        }

                        message = null;
                    }

                    File.WriteAllText(file, contents);
                }
                else
                {
                    error = String.Format(CultureInfo.InvariantCulture, "The file \"{0}\" does not exist.", file);
                    break;
                }
            }

            if (!String.IsNullOrEmpty(error))
            {
                if (BuildEngine != null)
                {
                    Log.LogError(error, null);
                }

                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
