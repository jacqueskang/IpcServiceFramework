using System;

namespace JKang.IpcServiceFramework.Hosting
{
    public class IpcHostingConfigurationException : Exception
    {
        public IpcHostingConfigurationException()
        { }

        public IpcHostingConfigurationException(string message)
            : base(message)
        { }

        public IpcHostingConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
