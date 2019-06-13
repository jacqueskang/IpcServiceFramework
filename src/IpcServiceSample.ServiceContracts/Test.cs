using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace IpcServiceSample.ServiceContracts
{
    public class Test : ITest
    {
        private int _sum;

        public int Sum => _sum;

        public Test(int sum)
        {
            _sum = sum;
        }
    }

    [KnownType(typeof(ITest))]
    public abstract class TestT : ITest
    {
        private int _sum;

        public int Sum => _sum;
    }
}
