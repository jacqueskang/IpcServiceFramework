using JKang.IpcServiceFramework.Client;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IpcClientServiceCollectionExtensions
    {
        public static IServiceCollection AddIpcClient<TContract, TIpcClientOptions>(
            this IServiceCollection services,
            IpcClientRegistration<TContract, TIpcClientOptions> registration)
            where TContract : class
            where TIpcClientOptions : IpcClientOptions
        {
            services
                .TryAddScoped<IIpcClientFactory<TContract>, IpcClientFactory<TContract, TIpcClientOptions>>();

            services.AddSingleton(registration);

            return services;
        }
    }
}
