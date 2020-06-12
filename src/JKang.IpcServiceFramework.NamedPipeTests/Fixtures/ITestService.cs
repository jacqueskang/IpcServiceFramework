using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.NamedPipeTests.Fixtures
{
    public interface ITestService
    {
        int PrimitiveTypes(bool a, byte b, sbyte c, char d, decimal e, double f, float g, int h, uint i, long j,
            ulong k, short l, ushort m);
        string StringType(string input);
        Complex ComplexType(Complex input);
        IEnumerable<Complex> ComplexTypeArray(IEnumerable<Complex> input);
        void ReturnVoid();
        DateTime DateTime(DateTime input);
        DateTimeStyles EnumType(DateTimeStyles input);
        byte[] ByteArray(byte[] input);
        T GenericMethod<T>(T input);
        Task<int> AsyncMethod();
        void ThrowException();
        ITestDto Abstraction(ITestDto input);
        void UnserializableInput(UnserializableObject input);
        UnserializableObject UnserializableOutput();
    }
}
