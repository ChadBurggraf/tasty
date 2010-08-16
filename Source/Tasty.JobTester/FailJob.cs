using System;
using System.Runtime.Serialization;
using Tasty.Jobs;

namespace Tasty.JobTester
{
    [DataContract(Namespace = Job.XmlNamespace)]
    [KnownType(typeof(InvalidOperationException))]
    public class FailJob : Job
    {
        public override string Name
        {
            get { return "Fail"; }
        }

        public override void Execute()
        {
            throw new InvalidOperationException();
        }
    }
}
