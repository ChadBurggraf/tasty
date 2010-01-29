//-----------------------------------------------------------------------
// <copyright file="Job.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Base <see cref="IJob"/> implementation.
    /// </summary>
    [DataContract(Namespace = Job.XmlNamespace)]
    public abstract class Job : IJob
    {
        /// <summary>
        /// Gets the XML namespace used during job serialization.
        /// </summary>
        public const string XmlNamespace = "http://tastycodes.com/tasty-dll/job/";

        /// <summary>
        /// Gets the job's display name.
        /// </summary>
        [IgnoreDataMember]
        public abstract string Name { get; }

        /// <summary>
        /// Gets the timeout, in miliseconds, the job is allowed to run for.
        /// When not overridden, defaults to 60,000 (1 minute).
        /// </summary>
        [IgnoreDataMember]
        public virtual long Timeout 
        { 
            get { return 60000; } 
        }

        /// <summary>
        /// Enqueues the job for execution.
        /// </summary>
        /// <returns>The job record that was persisted.</returns>
        public JobRecord Enqueue()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Serializes the job state for enqueueing.
        /// </summary>
        /// <returns>The serialized job data.</returns>
        public string Serialize()
        {
            DataContractSerializer serializer = new DataContractSerializer(this.GetType());
            StringBuilder sb = new StringBuilder();

            using (StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                using (XmlWriter xw = new XmlTextWriter(sw))
                {
                    serializer.WriteObject(xw, this);
                }
            }

            return sb.ToString();
        }
    }
}
