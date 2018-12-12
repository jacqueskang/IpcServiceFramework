using IpcServiceSample.ServiceContracts;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace IpcServiceSample.ConsoleServer
{
    public class SecureService : ISecureService
    {
        public Guid GenerateId()
        {
            return Guid.NewGuid();
        }
    }
}
