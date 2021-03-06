﻿//-----------------------------------------------------------------------
// <copyright file="TestService.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    
    /// <summary>
    /// Default implementation of <see cref="ITestService"/>.
    /// </summary>
    public class TestService : ITestService
    {
        /// <summary>
        /// Sums the given values.
        /// </summary>
        /// <param name="first">The first value.</param>
        /// <param name="second">The second value.</param>
        /// <returns>The sum of the given values.</returns>
        public double Sum(double first, double second)
        {
            return first + second;
        }
    }
}
