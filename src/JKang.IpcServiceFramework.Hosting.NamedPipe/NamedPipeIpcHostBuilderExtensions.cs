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
            string pipeName)
            where TContract : class
        {
            return builder.AddNamedPipeEndpoint<TContract>(options =>
            {
                options.PipeName = pipeName;
            });
        }

        public static IIpcHostBuilder AddNamedPipeEndpoint<TContract>(this IIpcHostBuilder builder,
            Action<NamedPipeIpcEndpointOptions> configure)
            where TContract : class
        {
            var options = new NamedPipeIpcEndpointOptions();
            configure?.Invoke(options);

            builder.AddIpcEndpoint(serviceProvider =>
            {
                IIpcMessageSerializer serializer = serviceProvider.GetRequiredService<IIpcMessageSerializer>();
                IValueConverter valueConverter = serviceProvider.GetRequiredService<IValueConverter>();
                ILogger<NamedPipeIpcEndpoint<TContract>> logger = serviceProvider
                    .GetRequiredService<ILogger<NamedPipeIpcEndpoint<TContract>>>();

                return new NamedPipeIpcEndpoint<TContract>(options, serializer, valueConverter, logger, serviceProvider);
            });

            return builder;
        }
    }
}
