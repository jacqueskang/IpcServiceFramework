using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Hosting
{
    public class IpcHostedService : IHostedService
    {
        private readonly IEnumerable<IIpcEndpoint> _endpoints;
        private readonly ILogger<IpcHostedService> _logger;

        public IpcHostedService(
            IEnumerable<IIpcEndpoint> endpoints,
            ILogger<IpcHostedService> logger)
        {
            _endpoints = endpoints;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (IIpcEndpoint endpoint in _endpoints)
            {
                await endpoint.StartAsync(cancellationToken).ConfigureAwait(false);
            }
            _logger.LogInformation("IPC hosted service started.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (IIpcEndpoint endpoint in _endpoints)
            {
                await endpoint.StopAsync(cancellationToken).ConfigureAwait(false);
            }
            _logger.LogInformation("IPC hosted service stopped.");
        }
    }
}
