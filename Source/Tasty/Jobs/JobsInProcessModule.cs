//-----------------------------------------------------------------------
// <copyright file="JobsInProcessModule.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Web;

    /// <summary>
    /// Implements <see cref="IHttpModule"/> to ensure that the <see cref="JobRunner"/>
    /// instance is started by the current <see cref="HttpApplication"/>.
    /// </summary>
    public class JobsInProcessModule : IHttpModule
    {
        /// <summary>
        /// Disposes of any unmanaged resources held by the module.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Initializes the module for the given application context.
        /// </summary>
        /// <param name="context">The application context to initialize the module for.</param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(delegate(object sender, EventArgs e)
            {
                if (JobRunner.Instance.IsGreen)
                {
                    JobRunner.Instance.Start();
                }
            });
        }
    }
}
