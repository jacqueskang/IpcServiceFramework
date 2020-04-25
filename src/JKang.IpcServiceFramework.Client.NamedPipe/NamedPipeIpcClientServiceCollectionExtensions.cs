using JKang.IpcServiceFramework.Client.NamedPipe;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NamedPipeIpcClientServiceCollectionExtensions
    {
        public static IServiceCollection AddNamedPipeIpcClient<TContract>(
            this IServiceCollection services, string pipeName)
            where TContract : class
        {
            return services.AddIpcClient((serializer, valueConverter) =>
            {
                return new NamedPipeIpcClient<TContract>(serializer, valueConverter, pipeName);
            });
        }
    }
}
