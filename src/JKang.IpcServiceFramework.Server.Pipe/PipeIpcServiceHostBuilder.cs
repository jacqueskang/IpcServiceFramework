using System;

namespace JKang.IpcServiceFramework
{
    public static class PipeIpcServiceHostBuilder
    {
        public static IIpcServiceHost Build(string pipeName, IServiceProvider serviceProvider)
        {
            return new PipeIpcServiceHost(pipeName, serviceProvider);
        }
    }
}
