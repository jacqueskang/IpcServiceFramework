using JKang.IpcServiceFramework.Client;
using JKang.IpcServiceFramework.Client.NamedPipe;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NamedPipeIpcClientServiceCollectionExtensions
    {
        public static IServiceCollection AddNamedPipeIpcClient<TContract>(
            this IServiceCollection services, string name, string pipeName)
            where TContract : class
        {
            return services.AddNamedPipeIpcClient<TContract>(name, (_, options) =>
            {
                options.PipeName = pipeName;
            });
        }

        public static IServiceCollection AddNamedPipeIpcClient<TContract>(
            this IServiceCollection services, string name,
            Action<IServiceProvider, NamedPipeIpcClientOptions> configureOptions)
            where TContract : class
        {
            services.AddIpcClient(new IpcClientRegistration<TContract, NamedPipeIpcClientOptions>(name,
                (_, options) => new NamedPipeIpcClient<TContract>(name, options), configureOptions));

            return services;
        }
    }
}
