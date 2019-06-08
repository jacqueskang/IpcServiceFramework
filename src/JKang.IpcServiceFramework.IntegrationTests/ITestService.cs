using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.IntegrationTests
{
    public interface ITestService
    {
        float AddFloat(float x, float y);
        Complex AddComplex(Complex x, Complex y);
        Complex SumComplexArray(IEnumerable<Complex> input);
        void DoNothing();
        DateTime ParseDate(string value, DateTimeStyles styles);
        byte[] ReverseBytes(byte[] input);
        T GetDefaultValue<T>();
        Task<long> WaitAsync(int milliseconds);
        int ExplicitInterfaceMember();
    }
}
