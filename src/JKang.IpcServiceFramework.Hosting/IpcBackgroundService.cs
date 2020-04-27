using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Hosting
{
    public sealed class IpcBackgroundService : BackgroundService
    {
        private readonly IEnumerable<IIpcEndpoint> _endpoints;
        private readonly ILogger<IpcBackgroundService> _logger;

        public IpcBackgroundService(
            IEnumerable<IIpcEndpoint> endpoints,
            ILogger<IpcBackgroundService> logger)
        {
            _endpoints = endpoints ?? throw new System.ArgumentNullException(nameof(endpoints));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (IIpcEndpoint endpoint in _endpoints)
            {
                await endpoint.ExecuteAsync(stoppingToken).ConfigureAwait(false);
                _logger.LogDebug("Endpoint '{EndpointName}' started.", endpoint.Name);
            }
            _logger.LogInformation("IPC background service started.");
        }

        public override void Dispose()
        {
            foreach (IIpcEndpoint endpoint in _endpoints)
            {
                endpoint.Dispose();
                _logger.LogDebug("Endpoint '{EndpointName}' disposed.", endpoint.Name);
            }

            base.Dispose();
            _logger.LogInformation("IPC background service disposed.");
        }
    }
}
