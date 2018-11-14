using JKang.IpcServiceFramework.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JKang.IpcServiceFramework
{
    public static class IpcServiceServiceCollectionExtensions
    {
        public static IIpcServiceBuilder AddIpc(this IServiceCollection services)
        {
            services
                .AddLogging()
                .AddScoped<IValueConverter, DefaultValueConverter>()
                .AddScoped<IIpcMessageSerializer, DefaultIpcMessageSerializer>();

            return new IpcServiceBuilder(services);
        }
    }
}
