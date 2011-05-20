

namespace Tasty
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public sealed class QueuedDictionaryEventArgs
    {
        public QueuedDictionaryEventArgs(IEnumerable<QueuedDictionaryEventArgsItem> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            this.Items = new List<QueuedDictionaryEventArgsItem>(items);
        }

        public IList<QueuedDictionaryEventArgsItem> Items { get; private set; }
    }
}
