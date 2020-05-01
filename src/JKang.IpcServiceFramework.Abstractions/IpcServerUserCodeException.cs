using System;
using System.Runtime.Serialization;

namespace JKang.IpcServiceFramework
{
    /// <summary>
    /// An exception that originated at the server, in user code.
    /// </summary>
    [Serializable]
    public class IpcServerUserCodeException : IpcServerException
    {
        public IpcServerUserCodeException()
        {
        }

        public IpcServerUserCodeException(string message)
            : base(message)
        {
        }

        public IpcServerUserCodeException(string message, string failureDetails)
            : base(message, failureDetails)
        {
        }

        public IpcServerUserCodeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected IpcServerUserCodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
