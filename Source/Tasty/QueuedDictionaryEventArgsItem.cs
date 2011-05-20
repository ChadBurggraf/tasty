

namespace Tasty
{
    using System;

    [Serializable]
    public sealed class QueuedDictionaryEventArgsItem
    {
        public QueuedDictionaryEventArgsItem(object key, object value, QueuedDictionaryAccess access)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", "key cannot be null.");
            }

            if (access == null)
            {
                throw new ArgumentNullException("access", "access cannot be null.");
            }

            this.Key = key;
            this.Value = value;
            this.Access = access;
        }

        public QueuedDictionaryAccess Access { get; private set; }

        public object Key { get; private set; }

        public object Value { get; private set; }
    }
}
