using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tasty.Jobs;

namespace Tasty.JobTester
{
    class Program
    {
        static void Main(string[] args)
        {
            //JobRunner.Instance.Start();

            new SendEmailJob().Enqueue();
            new FailJob().Enqueue();
            new LongJob().Enqueue();
            new FileJob().Enqueue();

            //Console.WriteLine("Press Ctl+C to exit.");
        }
    }
}
