//-----------------------------------------------------------------------
// <copyright file="EmailSentEventArgs.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;

    /// <summary>
    /// Event arguments for <see cref="Emailer.Sent"/> events.
    /// </summary>
    public class EmailSentEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the EmailSentEventArgs class.
        /// </summary>
        /// <param name="to">The destination address of the email.</param>
        public EmailSentEventArgs(string to)
        {
            if (String.IsNullOrEmpty(to))
            {
                throw new ArgumentNullException("to", "to must contain a value.");
            }

            this.To = to;
        }

        /// <summary>
        /// Gets the destination address of the email.
        /// </summary>
        public string To { get; private set; }
    }
}
