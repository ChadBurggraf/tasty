//-----------------------------------------------------------------------
// <copyright file="Email.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Build
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// Extends <see cref="Task"/> to send emails.
    /// </summary>
    public class Email : Task
    {
        private int? port;

        /// <summary>
        /// Gets or sets a collection of file paths to attach to the email.
        /// </summary>
        public ITaskItem[] Attachments { get; set; }

        /// <summary>
        /// Gets or sets a collection of addresses to BCC on the email.
        /// </summary>
        public ITaskItem[] Bcc { get; set; }

        /// <summary>
        /// Gets or sets the email's body.
        /// </summary>
        [Required]
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets a collection of addresses to CC on the email.
        /// </summary>
        public ITaskItem[] CC { get; set; }

        /// <summary>
        /// Gets or sets the address to send the email from.
        /// </summary>
        [Required]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the display name to use as the sender.
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email body is HTML.
        /// </summary>
        public bool IsHtml { get; set; }
        
        /// <summary>
        /// Gets or sets the password to use when connecting to the server.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the port number to connect to the server on.
        /// </summary>
        public int Port
        {
            get { return this.port ?? 25; }
            set { this.port = value; }
        }

        /// <summary>
        /// Gets or sets the collection of email address that were sent emails upon completion.
        /// </summary>
        public ITaskItem[] SentEmails { get; set; }

        /// <summary>
        /// Gets or sets the IP address or host name of the SMTP server to use.
        /// </summary>
        [Required]
        public string SmtpServer { get; set; }

        /// <summary>
        /// Gets or sets the email subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the collection of destination email addresses.
        /// </summary>
        [Required]
        public ITaskItem[] To { get; set; }

        /// <summary>
        /// Gets or sets the username to use when authenticating with the mail server.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use SSL when connecting to the mail server.
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>True if the task executed successfully, false otherwise.</returns>
        public override bool Execute()
        {
            List<string> sentEmails = new List<string>();

            SmtpClient client = new SmtpClient(this.SmtpServer, this.Port);
            client.EnableSsl = this.UseSsl;

            if (!String.IsNullOrEmpty(this.Username) && !String.IsNullOrEmpty(this.Password))
            {
                client.Credentials = new NetworkCredential(this.Username, this.Password);
            }

            using (MailMessage message = new MailMessage())
            {
                if (this.Attachments != null)
                {
                    foreach (var attachment in this.Attachments)
                    {
                        message.Attachments.Add(new Attachment(attachment.ItemSpec));
                    }
                }

                if (this.Bcc != null)
                {
                    foreach (var bcc in this.Bcc)
                    {
                        message.Bcc.Add(bcc.ItemSpec);
                    }
                }

                if (this.CC != null)
                {
                    foreach (var cc in this.CC)
                    {
                        message.CC.Add(cc.ItemSpec);
                    }
                }

                message.From = new MailAddress(this.From, this.FromName);
                message.Subject = this.Subject;
                message.Body = this.Body;
                message.IsBodyHtml = this.IsHtml;

                foreach (var to in this.To)
                {
                    message.To.Clear();
                    message.To.Add(to.ItemSpec);
                    client.Send(message);
                    sentEmails.Add(to.ItemSpec);
                }
            }

            this.SentEmails = sentEmails.Select(e => new TaskItem(e)).ToArray();
            return true;
        }
    }
}
