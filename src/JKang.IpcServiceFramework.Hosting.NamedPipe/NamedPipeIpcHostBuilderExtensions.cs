using JKang.IpcServiceFramework.Hosting.NamedPipe;
using JKang.IpcServiceFramework.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace JKang.IpcServiceFramework.Hosting
{
    public static class NamedPipeIpcHostBuilderExtensions
    {
        public static IIpcHostBuilder AddNamedPipeEndpoint<TContract>(this IIpcHostBuilder builder,
            string name, string pipeName)
            where TContract : class
        {
            return builder.AddNamedPipeEndpoint<TContract>(name, pipeName, null);
        }

        public static IIpcHostBuilder AddNamedPipeEndpoint<TContract>(this IIpcHostBuilder builder,
            string name, string pipeName, Action<NamedPipeIpcServiceEndpointOptions> configure)
            where TContract : class
        {
            var options = new NamedPipeIpcServiceEndpointOptions(pipeName);
            configure?.Invoke(options);

            builder.AddIpcEndpoint(serviceProvider =>
            {
                IIpcMessageSerializer serializer = serviceProvider.GetRequiredService<IIpcMessageSerializer>();
                IValueConverter valueConverter = serviceProvider.GetRequiredService<IValueConverter>();
                ILogger<NamedPipeIpcServiceEndpoint<TContract>> logger = serviceProvider
                    .GetRequiredService<ILogger<NamedPipeIpcServiceEndpoint<TContract>>>();

                return new NamedPipeIpcServiceEndpoint<TContract>(name, options, serializer, valueConverter, logger, serviceProvider);
            });

            return builder;
        }
    }
}
