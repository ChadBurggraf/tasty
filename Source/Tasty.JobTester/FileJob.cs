using System;
using System.IO;
using System.Runtime.Serialization;
using Tasty.Jobs;

namespace Tasty.JobTester
{
    [DataContract(Namespace = Job.XmlNamespace)]
    public class FileJob : Job
    {
        public override string Name
        {
            get { return "File"; }
        }

        public override void Execute()
        {
            File.AppendAllText("file.txt", "Hello World!\n\nThis file is a test.\n\nYou fucking rock, bro!");
        }
    }
}
