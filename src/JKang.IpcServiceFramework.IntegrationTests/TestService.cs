using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.IntegrationTests
{
    public class TestService : ITestService
    {
        public Complex AddComplex(Complex x, Complex y)
        {
            return x + y;
        }

        public float AddFloat(float x, float y)
        {
            return x + y;
        }

        public void DoNothing() { }

        public DateTime ParseDate(string value, DateTimeStyles styles)
        {
            return DateTime.Parse(value, CultureInfo.InvariantCulture, styles);
        }

        public Complex SumComplexArray(IEnumerable<Complex> input)
        {
            var result = new Complex();
            foreach (Complex value in input)
            {
                result += value;
            }
            return result;
        }

        public byte[] ReverseBytes(byte[] input)
        {
            return input.Reverse().ToArray();
        }

        public T GetDefaultValue<T>()
        {
            return default(T);
        }

        public async Task<long> WaitAsync(int milliseconds)
        {
            var sw = Stopwatch.StartNew();
            await Task.Delay(milliseconds);
            return sw.ElapsedMilliseconds;
        }
    }
}
