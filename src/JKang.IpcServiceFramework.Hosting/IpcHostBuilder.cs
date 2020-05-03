using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace JKang.IpcServiceFramework.Hosting
{
    internal class IpcHostBuilder : IIpcHostBuilder
    {
        private readonly IHostBuilder _hostBuilder;

        public IpcHostBuilder(IHostBuilder hostBuilder)
        {
            _hostBuilder = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            _hostBuilder.ConfigureServices((_, services) =>
            {
                services
                    .AddHostedService<IpcBackgroundService>();
            });
        }

        public IIpcHostBuilder AddIpcEndpoint(Func<IServiceProvider, IIpcEndpoint> endpointFactory)
        {
            if (endpointFactory is null)
            {
                throw new ArgumentNullException(nameof(endpointFactory));
            }

            _hostBuilder.ConfigureServices((_, services) =>
            {
                services.AddSingleton(endpointFactory);
            });

            return this;
        }
    }
}
