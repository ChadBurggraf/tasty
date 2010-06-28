

namespace Tasty.Test
{
    using System;
    using System.ServiceModel;
    using System.Runtime.Serialization;

    [ServiceContract]
    public interface ITestService
    {
        [OperationContract]
        double Sum(double first, double second);
    }

    public class TestService : ITestService
    {
        public double Sum(double first, double second)
        {
            return first + second;
        }
    }
}
