using System;

namespace JKang.IpcServiceFramework
{
    public static class IpcServiceHostBuilder
    {
        public static IIpcServiceHost Buid(string pipeName, IServiceProvider serviceProvider)
        {
            return new IpcServiceHost(pipeName, serviceProvider);
        }
    }
}
