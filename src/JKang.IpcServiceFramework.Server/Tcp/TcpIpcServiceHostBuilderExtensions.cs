using System.Net;
using JKang.IpcServiceFramework.Tcp;

namespace JKang.IpcServiceFramework
{
    public static class TcpIpcServiceHostBuilderExtensions
    {
        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, port));
        }
    }
}
