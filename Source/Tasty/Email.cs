

namespace Tasty
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Mail;
    using System.Threading;

    public class Email
    {
        private MailTemplate template;
        private int? port;

        /// <summary>
        /// Initializes a new instance of the Email class.
        /// </summary>
        /// <param name="template">The <see cref="MailTemplate"/> to use when processing the email(s).</param>
        public Email(MailTemplate template)
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
        }

        public event EventHandler AllSent;

        public event EventHandler<EmailSentEventArgs> Sent;

        public StringCollection Attachments { get; private set; }

        public StringCollection Bcc { get; private set; }

        public StringCollection CC { get; private set; }

        public string From { get; set; }

        public string FromDisplayName { get; set; }

        public string Password { get; set; }

        public int Port
        {
            get { return (int)(this.port ?? (this.port = 25)); }
            set { this.port = value; }
        }

        public string SmtpServer { get; set; }

        public string Subject { get; set; }

        public StringCollection To { get; private set; }

        public string Username { get; set; }

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
                    message.Body = this.template.Transform(model);
                    client.Send(message);

                    this.RaiseEvent(this.Sent, new EmailSentEventArgs(to));
                }
            }
        }

        /// <summary>
        /// Sends the email(s) current configured by this instance.
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

        private void ClientSendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.RaiseEvent(this.Sent, new EmailSentEventArgs((string)e.UserState));
        }

        private SmtpClient CreateClient()
        {
            SmtpClient client = new SmtpClient(this.SmtpServer, this.Port);
            client.EnableSsl = this.UseSsl;

            if (!String.IsNullOrEmpty(this.Username) && !String.IsNullOrEmpty(this.Password))
            {
                client.Credentials = new NetworkCredential(this.Username, this.Password);
            }

            return client;
        }

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

            return message;
        }

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
