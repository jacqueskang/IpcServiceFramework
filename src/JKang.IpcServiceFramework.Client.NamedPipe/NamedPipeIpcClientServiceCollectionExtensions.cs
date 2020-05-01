using JKang.IpcServiceFramework.Client.NamedPipe;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NamedPipeIpcClientServiceCollectionExtensions
    {
        public static IServiceCollection AddNamedPipeIpcClient<TContract>(
            this IServiceCollection services, string pipeName)
            where TContract : class
        {
            return services.AddNamedPipeIpcClient<TContract>(options =>
            {
                options.PipeName = pipeName;
            });
        }

        public static IServiceCollection AddNamedPipeIpcClient<TContract>(
            this IServiceCollection services, Action<NamedPipeIpcClientOptions> configure)
            where TContract : class
        {
            return services.AddIpcClient((serializer, valueConverter) =>
            {
                var options = new NamedPipeIpcClientOptions();
                configure?.Invoke(options);
                return new NamedPipeIpcClient<TContract>(options, serializer, valueConverter);
            });
        }

    }
}
