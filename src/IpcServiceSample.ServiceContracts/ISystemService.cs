using System;
using JKang.IpcServiceFramework.Services;

namespace IpcServiceSample.ServiceContracts
{
    public interface ISystemService
    {
        void DoNothing();
        string ConvertText(string text, TextStyle style);
        Guid GenerateId();
        byte[] ReverseBytes(byte[] input);
        string Printout<T>(T value);
        void SlowOperation();
        int TryGetInt(ITest test);
    }
    
    [KnowType(typeof(Test))]
    public interface ITest
    {
        int Sum { get; }     
    }
}
