//-----------------------------------------------------------------------
// <copyright file="JobRecordEventArgs.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Jobs
{
    using System;

    /// <summary>
    /// Event arguments passed to <see cref="JobRunner"/> events.
    /// </summary>
    public class JobRecordEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the JobRecordEventArgs class.
        /// </summary>
        /// <param name="record">The <see cref="JobRecord"/> the event is being raised for.</param>
        public JobRecordEventArgs(JobRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record", "record cannot be null.");
            }

            this.Record = record;
        }

        /// <summary>
        /// Gets or sets the <see cref="JobRecord"/> the event is being raised for.
        /// </summary>
        public JobRecord Record { get; protected set; }
    }
}
