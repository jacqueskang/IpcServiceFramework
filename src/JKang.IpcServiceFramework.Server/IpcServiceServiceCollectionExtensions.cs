using Microsoft.Extensions.DependencyInjection;

namespace JKang.IpcServiceFramework
{
    public static class IpcServiceServiceCollectionExtensions
    {
        public static IIpcServiceCollection AddIpc(this IServiceCollection services)
        {
            services
                .AddScoped<IIpcMessageSerializer, DefaultIpcMessageSerializer>();

            return new IpcServiceCollection(services);
        }
    }
}
