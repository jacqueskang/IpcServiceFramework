using IpcServiceSample.ServiceContracts;
using System;

namespace IpcServiceSample.Server
{
    public class InterProcessService : IInterProcessService
    {
        public string ReverseString(string input)
        {
            char[] charArray = input.ToCharArray();
            Array.Reverse(input.ToCharArray());
            return new string(charArray);
        }
    }
}
