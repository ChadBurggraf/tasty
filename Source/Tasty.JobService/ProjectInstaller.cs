//-----------------------------------------------------------------------
// <copyright file="ProjectInstaller.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.JobService
{
    using System;
    using System.ComponentModel;
    using System.Configuration.Install;

    /// <summary>
    /// <see cref="Installer"/> implementation for the Tasty Job Service.
    /// </summary>
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        /// <summary>
        /// Initializes a new instance of the ProjectInstaller class.
        /// </summary>
        public ProjectInstaller()
        {
            this.InitializeComponent();
        }
    }
}
