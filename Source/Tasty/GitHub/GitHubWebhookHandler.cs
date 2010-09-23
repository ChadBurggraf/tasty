//-----------------------------------------------------------------------
// <copyright file="GitHubWebhookHandler.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.GitHub
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using Tasty.Configuration;

    /// <summary>
    /// Implements <see cref="IHttpHandler"/> to receive GitHub post-receive hooks.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GitHubWebhookHandler : IHttpHandler
    {
        /// <summary>
        /// Gets the name of the POST payload parameter.
        /// </summary>
        public const string PayloadParameterName = "payload";

        /// <summary>
        /// Gets a value indicating whether another request can use the IHttpHandler instance.
        /// </summary>
        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the IHttpHandler interface.
        /// </summary>
        /// <param name="context">An HttpContext object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                if (!String.IsNullOrEmpty(context.Request.Params[PayloadParameterName]))
                {
                    GitHubWebhook hook = GitHubWebhook.Deserialize(context.Request.Params[PayloadParameterName]);

                    if (hook != null)
                    {
                        WebhookElement element = TastySettings.Section.GitHub.Webhooks.Where(h => h.Repository.Equals(hook.Repository.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                        if (element != null)
                        {
                            bool pass = String.IsNullOrEmpty(element.RefFilter);

                            if (!pass)
                            {
                                pass = Regex.IsMatch(hook.Ref, element.RefFilter, RegexOptions.IgnoreCase);
                            }

                            if (pass)
                            {
                                // Spawn this because MSBuild will warn about MTA aparment state, plus
                                // the script might run for a while.
                                Thread thread = new Thread(new ParameterizedThreadStart(delegate(object state)
                                {
                                    string projectFile = element.ProjectFile.RootPath(Path.GetDirectoryName(TastySettings.Section.ElementInformation.Source));
                                    GitHubWebhookMSBuildExecuter executer = new GitHubWebhookMSBuildExecuter(projectFile, hook);
                                    executer.SetTargets(element.Targets);
                                    executer.Execute();
                                }));

                                thread.SetApartmentState(ApartmentState.STA);
                                thread.Start();
                            }
                        }
                    }
                }
            }
            catch
            {
                if (!TastySettings.Section.GitHub.Webhooks.StifleExceptions)
                {
                    throw;
                }
            }
        }
    }
}
