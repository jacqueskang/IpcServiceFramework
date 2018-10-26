using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework
{
    public class IpcServiceHost : IIpcServiceHost
    {
        private readonly List<IpcServiceEndpoint> _endpoints;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IpcServiceHost> _logger;

        public IpcServiceHost(IEnumerable<IpcServiceEndpoint> endpoints, IServiceProvider serviceProvider)
        {
            _endpoints = endpoints.ToList();
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetService<ILogger<IpcServiceHost>>();
        }

        public void Run()
        {
            RunAsync().Wait();
        }

        public Task RunAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Task[] tasks = _endpoints
                .Select(endpoint => endpoint.ListenAsync(cancellationToken))
                .ToArray();
            return Task.WhenAll(tasks);
        }
    }
}
