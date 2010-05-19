using System;
using System.Configuration;
using System.Net.Mail;
using System.Runtime.Serialization;
using Tasty.Jobs;

namespace Tasty.JobTester
{
    [DataContract(Namespace = Job.XmlNamespace)]
    public class SendEmailJob : Job
    {
        public override string Name
        {
            get { return "Send Email"; }
        }

        public override void Execute()
        {
            MailMessage message = new MailMessage();
            message.To.Add("chad.burggraf@gmail.com");
            message.Subject = "Hello World!";
            message.Body = "This message is a test.\n\nYou fucking rock, bro!";

            SmtpClient client = new SmtpClient();
            client.EnableSsl = Boolean.Parse(ConfigurationManager.AppSettings["EnableSmtpSsl"]);
            client.Send(message);
        }
    }
}
