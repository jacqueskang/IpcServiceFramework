using System;
using System.Collections.Generic;
using System.Text;

namespace IpcServiceSample.ServiceContracts
{
    public interface ISystemService
    {
        void DoNothing();
        string ConvertText(string text, TextStyle style);
        Guid GenerateId();
        byte[] ReverseBytes(byte[] input);
    }
}
