using System.Net;
using System.Security.Cryptography.X509Certificates;
using JKang.IpcServiceFramework.Ssl;

namespace JKang.IpcServiceFramework
{
    public static class SslIpcServiceHostBuilderExtensions
    {
        public static IpcServiceHostBuilder AddSslEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, X509Certificate certificate)
            where TContract : class
        {
            return builder.AddEndpoint(new SslIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, certificate));
        }

        public static IpcServiceHostBuilder AddSslEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, X509Certificate certificate)
            where TContract : class
        {
            return builder.AddEndpoint(new SslIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, port, certificate));
        }
    }
}
