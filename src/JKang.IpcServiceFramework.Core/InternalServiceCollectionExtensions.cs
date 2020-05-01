using JKang.IpcServiceFramework.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InternalServiceCollectionExtensions
    {
        public static IServiceCollection TryAddIpcInternalServices(this IServiceCollection services)
        {
            services
                .TryAddScoped<IValueConverter, DefaultValueConverter>();

            services
                .TryAddScoped<IIpcMessageSerializer, DefaultIpcMessageSerializer>();

            return services;
        }
    }
}
