using System;
using System.Collections.Generic;
using System.Text;

namespace IpcServiceSample.ServiceContracts
{
    public interface ISecureService
    {
        Guid GenerateId();
    }
}
