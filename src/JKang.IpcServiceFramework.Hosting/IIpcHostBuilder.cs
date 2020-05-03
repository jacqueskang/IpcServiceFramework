using System;

namespace JKang.IpcServiceFramework.Hosting
{
    public interface IIpcHostBuilder
    {
        IIpcHostBuilder AddIpcEndpoint(Func<IServiceProvider, IIpcEndpoint> factory);
    }
}
