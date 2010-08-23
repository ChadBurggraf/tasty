//-----------------------------------------------------------------------
// <copyright file="DataSets.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
//     TODO: This code is adapted (almost verbatim) from an unknown source.
//     Please help me find and credit the original author!
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    /// <summary>
    /// Implements <see cref="IEqualityComparer{T}"/> for use with arbitrary lambda expressions.
    /// </summary>
    /// <typeparam name="T">The type of the objects to compare.</typeparam>
    public class LambdaComparer<T> : IEqualityComparer<T>
    {
        private Func<T, T, bool> comparer;
        private Func<T, int> hasher;

        /// <summary>
        /// Initializes a new instance of the LambdaComparer class.
        /// </summary>
        /// <param name="comparer">A lambda expression that can be used to compare objects for this instance's type.</param>
        public LambdaComparer(Func<T, T, bool> comparer)
            : this(comparer, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the LambdaComparer class.
        /// </summary>
        /// <param name="comparer">A lambda expression that can be used to compare objects for this instance's type.</param>
        /// <param name="hasher">A lambda expression that can be used to get the hash code of objects for this instance's type,
        /// or null to use each object's native <see cref="Object.GetHashCode()"/> method.</param>
        public LambdaComparer(Func<T, T, bool> comparer, Func<T, int> hasher)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer", "comparer cannot be null.");
            }

            this.comparer = comparer;
            this.hasher = hasher;
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>True if the objects are equal, false otherwise.</returns>
        public bool Equals(T x, T y)
        {
            return this.comparer(x, y);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The object to get a hash code for.</param>
        /// <returns>A hash code for the specified object.</returns>
        public int GetHashCode(T obj)
        {
            return this.hasher != null ? this.hasher(obj) : obj.GetHashCode();
        }
    }
}
