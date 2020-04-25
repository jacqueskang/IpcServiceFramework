using JKang.IpcServiceFramework.Client.Tcp;
using System.Net;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TcpIpcClientServiceCollectionExtensions
    {
        public static IServiceCollection AddTcpIpcClient<TContract>(
            this IServiceCollection services, IPAddress serverIp, int serverPort)
            where TContract : class
        {
            return services.AddIpcClient((serializer, valueConverter) =>
            {
                return new TcpIpcClient<TContract>(serializer, valueConverter, serverIp, serverPort);
            });
        }

        public static IServiceCollection AddTcpIpcClient<TContract>(
            this IServiceCollection services, TcpIpcClientOptions options)
            where TContract : class
        {
            return services.AddIpcClient((serializer, valueConverter) =>
            {
                return new TcpIpcClient<TContract>(serializer, valueConverter, options);
            });
        }
    }
}
