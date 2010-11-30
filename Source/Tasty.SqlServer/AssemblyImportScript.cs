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
            return Get(Assembly.GetAssembly(typeof(System.Linq.Queryable)).Location);
        }

        /// <summary>
        /// Gets the <see cref="Tasty.SqlServer"/> assembly import SQL script.
        /// </summary>
        /// <param name="systemAssembliesPath">The path to v3.5 reference assemblies on the SQL Server machine
        /// (e.g., C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.5).</param>
        /// <returns>The <see cref="Tasty.SqlServer"/> import script.</returns>
        public static string Get(string systemAssembliesPath)
        {
            systemAssembliesPath = systemAssembliesPath ?? String.Empty;

            if (systemAssembliesPath.EndsWith(@"\", StringComparison.Ordinal))
            {
                systemAssembliesPath = systemAssembliesPath.Substring(0, systemAssembliesPath.Length - 1);
            }

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

            return String.Format(CultureInfo.InvariantCulture, script, systemAssembliesPath, assmPath);
        }
    }
}
