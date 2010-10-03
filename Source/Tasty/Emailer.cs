//-----------------------------------------------------------------------
// <copyright file="Emailer.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.Net;
    using System.Net.Configuration;
    using System.Net.Mail;
    using System.Threading;

    /// <summary>
    /// Sends emails based on <see cref="MailModel"/>s and <see cref="MailTemplate"/>s.
    /// </summary>
    public class Emailer
    {
        private MailTemplate template;
        private int? port;
        private int sent;

        /// <summary>
        /// Initializes a new instance of the Emailer class.
        /// </summary>
        /// <param name="template">The <see cref="MailTemplate"/> to use when processing the email(s).</param>
        public Emailer(MailTemplate template)
        {
            if (template == null)
            {
                throw new ArgumentNullException("template", "template cannot be null.");
            }

            this.template = template;
            this.Attachments = new StringCollection();
            this.Bcc = new StringCollection();
            this.CC = new StringCollection();
            this.To = new StringCollection();

            this.InitializeFromConfiguration();
        }

        /// <summary>
        /// Event raised when emails have been sent to all of the addresses in the <see cref="To"/> collection.
        /// </summary>
        public event EventHandler AllSent;

        /// <summary>
        /// Event raised when an email is sent to a single destination address.
        /// </summary>
        public event EventHandler<EmailSentEventArgs> Sent;

        /// <summary>
        /// Gets the collection of file paths to attach to emails.
        /// </summary>
        public StringCollection Attachments { get; private set; }

        /// <summary>
        /// Gets the collection of addresses to BCC on emails.
        /// </summary>
        public StringCollection Bcc { get; private set; }

        /// <summary>
        /// Gets the collection of addresses to CC on emails.
        /// </summary>
        public StringCollection CC { get; private set; }

        /// <summary>
        /// Gets or sets the email address of the email sender.
        /// Defaults to value found in <mailSettings/> if not set.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the display name of the email sender.
        /// </summary>
        public string FromDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the password to use when connecting to the server.
        /// Defaults to value found in <mailSettings/> if not set.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the port to connect to the server on.
        /// Defaults to value found in <mailSettings/> if not set.
        /// </summary>
        public int Port
        {
            get { return (int)(this.port ?? (this.port = 25)); }
            set { this.port = value; }
        }

        /// <summary>
        /// Gets or sets the IP address or host name of the SMTP server.
        /// Defaults to value found in <mailSettings/> if not set.
        /// </summary>
        public string SmtpServer { get; set; }

        /// <summary>
        /// Gets or sets the email subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets the collection of destination addresses to send to.
        /// </summary>
        public StringCollection To { get; private set; }

        /// <summary>
        /// Gets or sets the username to use when authenticating with the server.
        /// Defaults to value found in <mailSettings/> if not set.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use SSL when connecting to the server.
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// Sends the email(s) current configured by this instance.
        /// </summary>
        /// <param name="model">The model to use when sending email.
        /// WARNING: The model's <see cref="MailModel.Email"/> property will be set for each recipient.</param>
        public void Send(MailModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model", "model cannot be null.");
            }

            this.Validate();
            SmtpClient client = this.CreateClient();

            using (MailMessage message = this.CreateMessage())
            {
                foreach (string to in this.To)
                {
                    model.Email = to;
                    message.To.Clear();
                    message.To.Add(to);
                    message.Body = this.template.Transform(model);
                    client.Send(message);

                    this.RaiseEvent(this.Sent, new EmailSentEventArgs(to));
                }

                this.RaiseEvent(this.AllSent, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Sends the email(s) current configured by this instance.
        /// WARNING: A giant assumption is made that changes to the <see cref="To"/> collection will not be made
        /// while this call is in progress, as well as that no other calls to <see cref="Send(MailModel)"/>
        /// or <see cref="SendAsync(MailModel)"/> are made on this instance while this call is in progress.
        /// </summary>
        /// <param name="model">The model to use when sending email.
        /// WARNING: The model's <see cref="MailModel.Email"/> property will be set for each recipient.</param>
        public void SendAsync(MailModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model", "model cannot be null.");
            }

            this.Validate();

            Thread thread = new Thread(new ParameterizedThreadStart(delegate(object state)
            {
                using (MailMessage message = this.CreateMessage())
                {
                    foreach (string to in this.To)
                    {
                        model.Email = to;
                        message.Body = this.template.Transform(model);

                        SmtpClient client = this.CreateClient();
                        client.SendCompleted += new SendCompletedEventHandler(this.ClientSendCompleted);
                        client.SendAsync(message, to);
                    }
                }
            }));

            thread.Start();
        }

        /// <summary>
        /// Initializes this instance's state from anything found in the configuration.
        /// </summary>
        private void InitializeFromConfiguration()
        {
            SmtpSection smtp = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;

            if (smtp != null)
            {
                this.From = smtp.From;

                if (smtp.Network != null)
                {
                    this.SmtpServer = smtp.Network.Host;
                    this.Port = smtp.Network.Port;
                    this.UserName = smtp.Network.UserName;
                    this.Password = smtp.Network.Password;
                }
            }
        }

        /// <summary>
        /// Raises an <see cref="SmtpClient"/>'s SendCompleted event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ClientSendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.sent++;
            this.RaiseEvent(this.Sent, new EmailSentEventArgs((string)e.UserState));

            if (this.sent == this.To.Count)
            {
                this.sent = 0;
                this.RaiseEvent(this.AllSent, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Creates a new <see cref="SmtpClient"/> from this instance's state.
        /// </summary>
        /// <returns>The created <see cref="SmtpClient"/>.</returns>
        private SmtpClient CreateClient()
        {
            SmtpClient client = new SmtpClient(this.SmtpServer, this.Port);
            client.EnableSsl = this.UseSsl;

            if (!String.IsNullOrEmpty(this.UserName) && !String.IsNullOrEmpty(this.Password))
            {
                client.Credentials = new NetworkCredential(this.UserName, this.Password);
            }

            return client;
        }

        /// <summary>
        /// Creates a new <see cref="MailMessage"/> object from this instance's state.
        /// </summary>
        /// <returns>The created <see cref="MailMessage"/>.</returns>
        private MailMessage CreateMessage()
        {
            MailMessage message = new MailMessage();

            foreach (string attachment in this.Attachments)
            {
                message.Attachments.Add(new Attachment(attachment));
            }

            foreach (string bcc in this.Bcc)
            {
                message.Bcc.Add(bcc);
            }

            foreach (string cc in this.CC)
            {
                message.CC.Add(cc);
            }

            message.From = new MailAddress(this.From, this.FromDisplayName);
            message.Subject = this.Subject;
            message.IsBodyHtml = true;

            return message;
        }

        /// <summary>
        /// Validates this instance's state before sending.
        /// </summary>
        private void Validate()
        {
            if (String.IsNullOrEmpty(this.From))
            {
                throw new InvalidOperationException("From must be set to a value before sending.");
            }

            if (String.IsNullOrEmpty(this.SmtpServer))
            {
                throw new InvalidOperationException("SmtpServer must be set to a value before sending.");
            }

            if (this.To.Count == 0)
            {
                throw new InvalidOperationException("To must contain at least one email address before sending.");
            }
        }
    }
}
