using IpcServiceSample.ServiceContracts;
using System;

namespace IpcServiceSample.Server
{
    public class InterProcessService : IInterProcessService
    {
        public string ReverseString(string input)
        {
            return new string(Array.Reverse(input.ToCharArray()));
        }
    }
}
