using System;

namespace JKang.IpcServiceFramework
{
    public abstract class IpcServiceEndpoint
    {
        protected IpcServiceEndpoint(string name, IServiceProvider serviceProvider, string protocol)
        {
            Name = name;
            ServiceProvider = serviceProvider;
            Protocol = protocol;
        }

        public string Name { get; }
        public IServiceProvider ServiceProvider { get; }
        public string Protocol { get; }

        public abstract void Listen();
    }
}