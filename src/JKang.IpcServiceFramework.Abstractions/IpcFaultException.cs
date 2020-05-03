using System;

namespace JKang.IpcServiceFramework
{
    /// <summary>
    /// Represent an exception from server
    /// </summary>
    public class IpcFaultException : IpcException
    {
        public IpcFaultException(IpcStatus status)
        {
            Status = status;
        }

        public IpcFaultException(IpcStatus status, string message)
            : base(message)
        {
            Status = status;
        }

        public IpcFaultException(IpcStatus status, string message, Exception innerException)
            : base(message, innerException)
        {
            Status = status;
        }

        public IpcStatus Status { get; }
    }
}
