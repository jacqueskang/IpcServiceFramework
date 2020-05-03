using System;

namespace JKang.IpcServiceFramework
{
    public class IpcSerializationException : IpcException
    {
        public IpcSerializationException()
        { }

        public IpcSerializationException(string message)
            : base(message)
        { }

        public IpcSerializationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
