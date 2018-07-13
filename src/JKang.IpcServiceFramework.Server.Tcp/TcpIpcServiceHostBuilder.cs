using System;
using System.Net;

namespace JKang.IpcServiceFramework
{
    public static class TcpIpcServiceHostBuilder
    {
        public static IIpcServiceHost Build(IPAddress endpoint, int port, IServiceProvider serviceProvider)
        {
            return new TcpIpcServiceHost(endpoint, port, serviceProvider);
        }
    }
}
