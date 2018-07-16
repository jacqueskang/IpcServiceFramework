using Microsoft.Extensions.DependencyInjection;
using System;

namespace JKang.IpcServiceFramework
{
    public static class NamedPipeIpcServiceCollectionExtensions
    {
        public static IIpcServiceBuilder AddNamedPipe(this IIpcServiceBuilder builder)
        {
            return builder.AddNamedPipe(null);
        }

        public static IIpcServiceBuilder AddNamedPipe(this IIpcServiceBuilder builder, Action<NamedPipeOptions> configure)
        {
            var options = new NamedPipeOptions();
            configure?.Invoke(options);

            builder.Services
                .AddSingleton(options)
                ;

            return builder;
        }
    }
}
