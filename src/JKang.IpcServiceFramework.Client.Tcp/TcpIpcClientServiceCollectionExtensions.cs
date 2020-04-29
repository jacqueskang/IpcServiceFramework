using JKang.IpcServiceFramework.Client.Tcp;
using System;
using System.Net;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TcpIpcClientServiceCollectionExtensions
    {
        public static IServiceCollection AddTcpIpcClient<TContract>(
            this IServiceCollection services, IPAddress serverIp, int serverPort)
            where TContract : class
        {
            return services.AddTcpIpcClient<TContract>(options =>
            {
                options.ServerIp = serverIp;
                options.ServerPort = serverPort;
            });
        }

        public static IServiceCollection AddTcpIpcClient<TContract>(
            this IServiceCollection services, Action<TcpIpcClientOptions> configure)
            where TContract : class
        {
            var options = new TcpIpcClientOptions();
            configure?.Invoke(options);

            return services.AddIpcClient((serializer, valueConverter) =>
            {
                return new TcpIpcClient<TContract>(options, serializer, valueConverter);
            });
        }
    }
}
