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
            var jobRunner = JobRunner.GetInstance();
            jobRunner.AllFinished += new EventHandler(jobRunner_AllFinished);
            jobRunner.CancelJob += new EventHandler<JobRecordEventArgs>(jobRunner_CancelJob);
            jobRunner.DequeueJob += new EventHandler<JobRecordEventArgs>(jobRunner_DequeueJob);
            jobRunner.Error += new EventHandler<JobErrorEventArgs>(jobRunner_Error);
            jobRunner.ExecuteScheduledJob += new EventHandler<JobRecordEventArgs>(jobRunner_ExecuteScheduledJob);
            jobRunner.FinishJob += new EventHandler<JobRecordEventArgs>(jobRunner_FinishJob);
            jobRunner.TimeoutJob += new EventHandler<JobRecordEventArgs>(jobRunner_TimeoutJob);
            jobRunner.Start();

            new FailJob().Enqueue();
            new LongJob().Enqueue();
            new FileJob().Enqueue();

            Console.WriteLine("Press Ctl+C to exit.");
        }

        static void jobRunner_TimeoutJob(object sender, JobRecordEventArgs e)
        {
            Console.WriteLine("Timeout job: {0} ({1})", e.Record.Name, e.Record.Id);
        }

        static void jobRunner_FinishJob(object sender, JobRecordEventArgs e)
        {
            Console.WriteLine("Finish job: {0} ({1})", e.Record.Name, e.Record.Id);
        }

        static void jobRunner_ExecuteScheduledJob(object sender, JobRecordEventArgs e)
        {
            Console.WriteLine("Execute scheduled job: {0} ({1})", e.Record.Name, e.Record.Id);
        }

        static void jobRunner_Error(object sender, JobErrorEventArgs e)
        {
            if (e.Exception != null)
            {
                string jobName = e.Record != null ? e.Record.Name : "Unknown";
                int jobId = e.Record != null && e.Record.Id != null ? e.Record.Id.Value : 0;

                Console.WriteLine("An error occurred during execution of job: {0} ({1})", jobName, jobId);
                Console.WriteLine("The error message was: " + e.Exception.Message);
            }
            else if (e.Record != null)
            {
                Console.WriteLine("An error occurred during execution of job: {0}", e.Record.Name);
            }
            else
            {
                Console.WriteLine("An unexpected error occurred.");
            }
        }

        static void jobRunner_DequeueJob(object sender, JobRecordEventArgs e)
        {
            Console.WriteLine("Dequeue job: {0} ({1})", e.Record.Name, e.Record.Id);
        }

        static void jobRunner_CancelJob(object sender, JobRecordEventArgs e)
        {
            Console.WriteLine("Cancel job: {0} ({1})", e.Record.Name, e.Record.Id);
        }

        static void jobRunner_AllFinished(object sender, EventArgs e)
        {
            Console.WriteLine("All finished.");
        }
    }
}
