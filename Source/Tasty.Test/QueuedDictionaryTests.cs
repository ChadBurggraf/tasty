//-----------------------------------------------------------------------
// <copyright file="QueuedDictionaryTests.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Queued dictionary tests.
    /// </summary>
    [TestClass]
    public sealed class QueuedDictionaryTests
    {
        /// <summary>
        /// LRU access count eviction tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", Justification = "Using local to increase access count.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void QueuedDictionaryEvictAccessCountLru()
        {
            QueuedDictionary<Guid, int> dict = new QueuedDictionary<Guid, int>(3, QueuedDictionaryAccessCompareMode.AccessCountAscending);
            Guid one = Guid.NewGuid(), two = Guid.NewGuid(), three = Guid.NewGuid();

            dict[one] = 1;
            dict[two] = 2;
            dict[three] = 3;

            for (int i = 0; i < 5; i++)
            {
                int value = dict[one];
                value = dict[two];
                value = dict[three];
            }

            for (int i = 0; i < 25; i++)
            {
                dict[Guid.NewGuid()] = i;
            }

            Assert.AreEqual(3, dict.Count);
            Assert.IsTrue(dict.ContainsKey(one));
            Assert.IsTrue(dict.ContainsKey(two));
            Assert.IsTrue(dict.ContainsKey(three));
        }

        /// <summary>
        /// MRU access count eviction tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", Justification = "Using local to increase access count.")]
        public void QueuedDictionaryEvictAccessCountMru()
        {
            QueuedDictionary<Guid, int> dict = new QueuedDictionary<Guid, int>(3, QueuedDictionaryAccessCompareMode.AccessCountDescending);
            Guid one = Guid.NewGuid(), two = Guid.NewGuid(), three = Guid.NewGuid();

            dict[one] = 1;
            dict[two] = 2;
            dict[three] = 3;

            for (int i = 0; i < 5; i++)
            {
                int value = dict[one];
                value = dict[two];
                value = dict[three];
            }

            for (int i = 0; i < 25; i++)
            {
                dict[Guid.NewGuid()] = i;
            }

            Assert.AreEqual(3, dict.Count);
            Assert.IsFalse(dict.ContainsKey(one));
            Assert.IsFalse(dict.ContainsKey(two));
            Assert.IsFalse(dict.ContainsKey(three));
        }

        /// <summary>
        /// LRU access date eviction tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void QueuedDictionaryEvictAccessDateLru()
        {
            QueuedDictionary<Guid, int> dict = new QueuedDictionary<Guid, int>(3, QueuedDictionaryAccessCompareMode.LastAccessDateAscending);
            Stack<Guid> stack = new Stack<Guid>();

            for (int i = 0; i < 25; i++)
            {
                Guid key = Guid.NewGuid();
                dict[key] = i;
                stack.Push(key);
                Thread.Sleep(1);
            }

            Assert.AreEqual(3, dict.Count);
            Assert.IsTrue(dict.ContainsKey(stack.Pop()));
            Assert.IsTrue(dict.ContainsKey(stack.Pop()));
            Assert.IsTrue(dict.ContainsKey(stack.Pop()));
        }

        /// <summary>
        /// MRU access date eviction tests.
        /// </summary>
        [TestMethod]
        public void QueuedDictionaryEvictAccessDateMru()
        {
            QueuedDictionary<Guid, int> dict = new QueuedDictionary<Guid, int>(3, QueuedDictionaryAccessCompareMode.LastAccessDateDescending);
            Queue<Guid> queue = new Queue<Guid>();

            for (int i = 0; i < 25; i++)
            {
                Guid key = Guid.NewGuid();
                dict[key] = i;
                queue.Enqueue(key);
                Thread.Sleep(1);
            }

            Assert.AreEqual(3, dict.Count);
            Assert.IsTrue(dict.ContainsKey(queue.Dequeue()));
            Assert.IsTrue(dict.ContainsKey(queue.Dequeue()));
            Assert.IsTrue(dict.ContainsKey(queue.Dequeue()));
        }

        /// <summary>
        /// LRU creation date eviction tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void QueuedDictionaryEvictCreationDateLru()
        {
            QueuedDictionary<Guid, int> dict = new QueuedDictionary<Guid, int>(3, QueuedDictionaryAccessCompareMode.CreationDateAscending);
            Stack<Guid> stack = new Stack<Guid>();

            for (int i = 0; i < 25; i++)
            {
                Guid key = Guid.NewGuid();
                dict[key] = i;
                stack.Push(key);
                Thread.Sleep(1);
            }

            Assert.AreEqual(3, dict.Count);
            Assert.IsTrue(dict.ContainsKey(stack.Pop()));
            Assert.IsTrue(dict.ContainsKey(stack.Pop()));
            Assert.IsTrue(dict.ContainsKey(stack.Pop()));
        }

        /// <summary>
        /// MRU creation date eviction tests.
        /// </summary>
        [TestMethod]
        public void QueuedDictionaryEvictCreationDateMru()
        {
            QueuedDictionary<Guid, int> dict = new QueuedDictionary<Guid, int>(3, QueuedDictionaryAccessCompareMode.CreationDateDescending);
            Queue<Guid> queue = new Queue<Guid>();

            for (int i = 0; i < 25; i++)
            {
                Guid key = Guid.NewGuid();
                dict[key] = i;
                queue.Enqueue(key);
                Thread.Sleep(1);
            }

            Assert.AreEqual(3, dict.Count);
            Assert.IsTrue(dict.ContainsKey(queue.Dequeue()));
            Assert.IsTrue(dict.ContainsKey(queue.Dequeue()));
            Assert.IsTrue(dict.ContainsKey(queue.Dequeue()));
        }
    }
}
