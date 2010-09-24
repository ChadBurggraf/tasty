

namespace Tasty
{
    using System;

    public class EmailSentEventArgs : EventArgs
    {
        public EmailSentEventArgs(string to)
        {
            if (String.IsNullOrEmpty(to))
            {
                throw new ArgumentNullException("to", "to must contain a value.");
            }

            this.To = to;
        }

        public string To { get; private set; }
    }
}
