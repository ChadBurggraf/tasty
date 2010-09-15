//-----------------------------------------------------------------------
// <copyright file="TastyJobService.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.JobService
{
    using System;
    using System.ServiceProcess;

    /// <summary>
    /// The main execution handler for TastyJobService.exe.
    /// </summary>
    public static class TastyJobService
    {
        /// <summary>
        /// Main execution method.
        /// </summary>
        public static void Main()
        {
            ServiceBase.Run(new ServiceBase[] { new Service() });
        }
    }
}
