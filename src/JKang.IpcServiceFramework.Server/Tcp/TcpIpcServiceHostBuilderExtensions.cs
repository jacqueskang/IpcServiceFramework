using System.Net;
using JKang.IpcServiceFramework.Tcp;

namespace JKang.IpcServiceFramework
{
    public static class TcpIpcServiceHostBuilderExtensions
    {
        public static IpcServiceHostBuilder AddTcpEndpoint(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port)
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint(name, builder.ServiceProvider, ipEndpoint, port));
        }
    }
}
