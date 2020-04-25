using System;
using System.Collections.Generic;
using System.Text;

namespace IpcServiceSample.ServiceContracts
{
    public interface ITestService
    {
        Guid GenerateId();
    }
}
