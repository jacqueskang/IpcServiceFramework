using JKang.IpcServiceFramework.Client;
using JKang.IpcServiceFramework.Services;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IpcClientServiceCollectionExtensions
    {
        public static IServiceCollection AddIpcClient<TContract>(this IServiceCollection services,
            Func<IIpcMessageSerializer, IValueConverter, IIpcClient<TContract>> factory)
            where TContract : class
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            services
                .TryAddIpcInternalServices()
                ;

            services.AddScoped(serviceProvider =>
            {
                IIpcMessageSerializer serializer = serviceProvider.GetRequiredService<IIpcMessageSerializer>();
                IValueConverter valueConverter = serviceProvider.GetRequiredService<IValueConverter>();
                return factory(serializer, valueConverter);
            });

            return services;
        }
    }
}
