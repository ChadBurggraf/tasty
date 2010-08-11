using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Tasty.Configuration;
using Tasty.Jobs;

namespace Tasty.Test
{
    [DataContract(Namespace = Job.XmlNamespace)]
    internal class TestIdJob : Job
    {
        public TestIdJob()
        {
            Id = Guid.NewGuid();
        }

        [DataMember]
        public Guid Id { get; set; }

        public override string Name
        {
            get { return "Test ID Job"; }
        }

        public override long Timeout
        {
            get
            {
                return 10; // 10 ms.
            }
        }

        public override void Execute()
        {
        }
    }

    [DataContract(Namespace = Job.XmlNamespace)]
    internal class TestQuickJob : Job
    {
        public override string Name
        {
            get { return "Test Quick Job"; }
        }

        public override void Execute()
        {
        }
    }

    /*[DataContract(Namespace = Job.XmlNamespace)]
    internal class TestScheduledJob : ScheduledJob
    {
        public override string Name
        {
            get { return "Test Scheduled Job"; }
        }

        public override void Execute()
        {
        }
    }*/

    [DataContract(Namespace = Job.XmlNamespace)]
    internal class TestSlowJob : Job
    {
        public override string Name
        {
            get { return "Test Slow Job"; }
        }

        public override void Execute()
        {
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 10);
        }
    }

    [DataContract(Namespace = Job.XmlNamespace)]
    internal class TestTimeoutJob : Job
    {
        public override string Name
        {
            get { return "Test Timeout Job"; }
        }

        public override long Timeout
        {
            get
            {
                return TastySettings.Section.Jobs.Heartbeat;
            }
        }

        public override void Execute()
        {
            Thread.Sleep(TastySettings.Section.Jobs.Heartbeat * 10);
        }
    }
}
