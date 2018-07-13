using JKang.IpcServiceFramework.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JKang.IpcServiceFramework
{
    public static class IpcServiceServiceCollectionExtensions
    {
        public static IIpcServiceBuilder AddIpc(this IServiceCollection services)
        {
            services
                .AddScoped<IValueConverter, DefaultValueConverter>()
                .AddScoped<IIpcMessageSerializer, DefaultIpcMessageSerializer>();

            return new IpcServiceBuilder(services);
        }

        [Obsolete("Use AddIpc().AddNamedPipe(configure) instead")]
        public static IIpcServiceBuilder AddIpc(this IServiceCollection services, Action<IpcServiceOptions> configure)
        {
            var options = new IpcServiceOptions();
            configure?.Invoke(options);

            return services.AddIpc(options);
        }

        [Obsolete("Use AddIpc().AddNamedPipe(configure) instead")]
        public static IIpcServiceBuilder AddIpc(this IServiceCollection services, IpcServiceOptions options)
        {
            return AddIpc(services)
                .AddNamedPipe(o => o.ThreadCount = options.ThreadCount);
        }
    }
}
