using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Hosting
{
    public sealed class IpcHostedService : BackgroundService
    {
        private readonly IEnumerable<IIpcEndpoint> _endpoints;
        private readonly ILogger<IpcHostedService> _logger;

        public IpcHostedService(
            IEnumerable<IIpcEndpoint> endpoints,
            ILogger<IpcHostedService> logger)
        {
            _endpoints = endpoints ?? throw new System.ArgumentNullException(nameof(endpoints));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (IIpcEndpoint endpoint in _endpoints)
            {
                await endpoint.ExecuteAsync(stoppingToken).ConfigureAwait(false);
                _logger.LogDebug("Started endpoint {EndpointName}.", endpoint.Name);
            }
        }

        public override void Dispose()
        {
            foreach (IIpcEndpoint endpoint in _endpoints)
            {
                endpoint.Dispose();
            }

            base.Dispose();
        }
    }
}
