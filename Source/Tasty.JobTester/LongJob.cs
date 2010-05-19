using System;
using System.Runtime.Serialization;
using System.Threading;
using Tasty.Configuration;
using Tasty.Jobs;

namespace Tasty.JobTester
{
    [DataContract(Namespace = Job.XmlNamespace)]
    public class LongJob : Job
    {
        public override string Name
        {
            get { return "Long"; }
        }

        public override long Timeout
        {
            get { return TastySettings.Section.Jobs.Heartbeat * 2; }
        }

        public override void Execute()
        {
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 5);
        }
    }
}
