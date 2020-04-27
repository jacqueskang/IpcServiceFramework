using IpcServiceSample.ServiceContracts;
using System;

namespace IpcServiceSample.Server
{
    public class InterProcessService : IInterProcessService
    {
        public string ReverseString(string input)
        {
            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
