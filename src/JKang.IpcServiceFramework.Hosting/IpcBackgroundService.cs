using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
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

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.WhenAll(_endpoints.Select(x => x.ExecuteAsync(stoppingToken)));
        }

        public override void Dispose()
        {
            foreach (IIpcEndpoint endpoint in _endpoints)
            {
                endpoint.Dispose();
            }

            base.Dispose();
            _logger.LogInformation("IPC background service disposed.");
        }
    }
}
