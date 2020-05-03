using System;

namespace JKang.IpcServiceFramework
{
    public class IpcCommunicationException : IpcException
    {
        public IpcCommunicationException()
        { }

        public IpcCommunicationException(string message)
            : base(message)
        { }

        public IpcCommunicationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
