//-----------------------------------------------------------------------
// <copyright file="AssemblyImportScript.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.SqlServer
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Provides access to the <see cref="Tasty.SqlServer"/> assembly import SQL script.
    /// </summary>
    public static class AssemblyImportScript
    {
        /// <summary>
        /// Gets the <see cref="Tasty.SqlServer"/> assembly import SQL script.
        /// </summary>
        /// <returns>The <see cref="Tasty.SqlServer"/> import script.</returns>
        public static string Get()
        {
            Assembly currentAssm = Assembly.GetAssembly(typeof(AssemblyImportScript));
            string assmPath = currentAssm.Location;
            string script;

            using (Stream stream = currentAssm.GetManifestResourceStream("Tasty.SqlServer.AssemblyImport.sql"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    script = reader.ReadToEnd();
                }
            }

            return String.Format(CultureInfo.InvariantCulture, script, assmPath);
        }
    }
}
