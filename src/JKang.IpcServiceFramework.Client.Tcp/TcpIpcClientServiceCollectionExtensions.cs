using JKang.IpcServiceFramework.Client;
using JKang.IpcServiceFramework.Client.Tcp;
using System;
using System.Net;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TcpIpcClientServiceCollectionExtensions
    {
        public static IServiceCollection AddTcpIpcClient<TContract>(
            this IServiceCollection services, string name, IPAddress serverIp, int serverPort)
            where TContract : class
        {
            return services.AddTcpIpcClient<TContract>(name, (_, options) =>
            {
                options.ServerIp = serverIp;
                options.ServerPort = serverPort;
            });
        }

        public static IServiceCollection AddTcpIpcClient<TContract>(
            this IServiceCollection services, string name,
            Action<IServiceProvider, TcpIpcClientOptions> configureOptions)
            where TContract : class
        {
            services.AddIpcClient(new IpcClientRegistration<TContract, TcpIpcClientOptions>(name,
                (_, options) => new TcpIpcClient<TContract>(name, options), configureOptions));

            return services;
        }
    }
}
