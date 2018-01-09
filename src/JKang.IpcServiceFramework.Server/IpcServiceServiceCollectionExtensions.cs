using JKang.IpcServiceFramework.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JKang.IpcServiceFramework
{
    public static class IpcServiceServiceCollectionExtensions
    {
        public static IIpcServiceCollection AddIpc(this IServiceCollection services)
        {
            return services.AddIpc(new IpcServiceOptions());
        }

        public static IIpcServiceCollection AddIpc(this IServiceCollection services, Action<IpcServiceOptions> configure)
        {
            var options = new IpcServiceOptions();
            configure?.Invoke(options);
            return services.AddIpc(options);
        }

        public static IIpcServiceCollection AddIpc(this IServiceCollection services, IpcServiceOptions options)
        {
            services
                .AddSingleton(options)
                .AddScoped<IValueConverter, DefaultValueConverter>()
                .AddScoped<IIpcMessageSerializer, DefaultIpcMessageSerializer>()
                ;

            return new IpcServiceCollection(services);
        }
    }
}
