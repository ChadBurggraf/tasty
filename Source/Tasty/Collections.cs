//-----------------------------------------------------------------------
// <copyright file="Collections.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides extensions and helpers on collections.
    /// </summary>
    public static class Collections
    {
        /// <summary>
        /// Computes the covariance of the collection with the given comparison collection.
        /// </summary>
        /// <typeparam name="T">The collection's item type.</typeparam>
        /// <typeparam name="U">The comparison collection's item type.</typeparam>
        /// <param name="values">The collection to compute the covariance of.</param>
        /// <param name="comparison">The comparison collection to compute the covariance of.</param>
        /// <returns>The covariance of the collection with the comparison collection.</returns>
        public static double Covariance<T, U>(this IEnumerable<T> values, IEnumerable<U> comparison)
        {
            double pearson;
            return values.Covariance(comparison, out pearson);
        }

        /// <summary>
        /// Computes the covariance of the collection with the given comparison collection.
        /// </summary>
        /// <typeparam name="T">The collection's item type.</typeparam>
        /// <typeparam name="U">The comparison collection's item type.</typeparam>
        /// <param name="values">The collection to compute the covariance of.</param>
        /// <param name="comparison">The comparison collection to compute the covariance of.</param>
        /// <param name="pearson">Contains the pearson value once the calculation is complete.</param>
        /// <returns>The covariance of the collection with the comparison collection.</returns>
        public static double Covariance<T, U>(this IEnumerable<T> values, IEnumerable<U> comparison, out double pearson)
        {
            pearson = 0;

            if (values != null && values.Count() > 0 && comparison != null)
            {
                int count = values.Count();

                if (count != comparison.Count())
                {
                    throw new ArgumentException("comparison", "the comparison collection must be the same length as the primary collection.");
                }

                IEnumerable<double> vcoll = typeof(IEnumerable<double>).IsAssignableFrom(values.GetType()) ?
                    (IEnumerable<double>)values :
                    values.Select(v => Convert.ToDouble(v));

                IEnumerable<double> ccoll = typeof(IEnumerable<double>).IsAssignableFrom(values.GetType()) ?
                    (IEnumerable<double>)comparison :
                    comparison.Select(v => Convert.ToDouble(v));

                double vavg = vcoll.Average();
                double vstdv = Math.Sqrt(vcoll.Select(v => Math.Pow(v - vavg, 2)).Average());
                double cavg = ccoll.Average();
                double cstdv = Math.Sqrt(ccoll.Select(v => Math.Pow(v - cavg, 2)).Average());

                double covariance = vcoll
                    .Select((v, i) => (v - vavg) * (ccoll.ElementAt(i) - cavg))
                    .Sum();

                covariance /= count;
                pearson = vstdv == 0 || cstdv == 0 ? 0 : covariance / (vstdv * cstdv);

                return covariance;
            }

            return 0;
        }

        /// <summary>
        /// Computes the standard deviation of the values in the collection.
        /// </summary>
        /// <typeparam name="T">The collection's item type.</typeparam>
        /// <param name="values">The collection of values to compute the standard deviation of.</param>
        /// <returns>The standard deviation.</returns>
        public static double StandardDeviation<T>(this IEnumerable<T> values)
        {
            return Math.Sqrt(values.Variance());
        }

        /// <summary>
        /// Computes the variance of the values in the collection.
        /// </summary>
        /// <typeparam name="T">The collection's item type.</typeparam>
        /// <param name="values">The collection of values to compute the variance of.</param>
        /// <returns>The variance.</returns>
        public static double Variance<T>(this IEnumerable<T> values)
        {
            if (values != null && values.Count() > 0)
            {
                IEnumerable<double> coll = typeof(IEnumerable<double>).IsAssignableFrom(values.GetType()) ?
                    (IEnumerable<double>)values :
                    values.Select(v => Convert.ToDouble(v));

                double average = coll.Average();

                return coll
                    .Select(v => Math.Pow(v - average, 2))
                    .Average();
            }

            return 0;
        }
    }
}
