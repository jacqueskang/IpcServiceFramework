using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Tcp
{
    public class TcpIpcServiceEndpoint<TContract> : IpcServiceEndpoint<TContract>
        where TContract : class
    {
        private readonly ILogger<TcpIpcServiceEndpoint<TContract>> _logger;

        public int Port { get; private set; }

        private readonly TcpListener _listener;

        public TcpIpcServiceEndpoint(String name, IServiceProvider serviceProvider, IPAddress ipEndpoint, int port = 0)
            : base(name, serviceProvider)
        {
            _listener = new TcpListener(ipEndpoint, port);
            _logger = serviceProvider.GetService<ILogger<TcpIpcServiceEndpoint<TContract>>>();
            Port = port;
        }

        public override Task ListenAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _listener.Start();

            // If port is dynamically assigned, get the port number after start
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;

            cancellationToken.Register(() =>
            {
                _listener.Stop();
            });

            return Task.Run(async () =>
            {
                try
                {
                    _logger.LogDebug($"Endpoint '{Name}' listening on port {Port}...");
                    while (true)
                    {
                        TcpClient client = await _listener.AcceptTcpClientAsync();
                        NetworkStream server = client.GetStream();
                        await ProcessAsync(server, _logger, cancellationToken);
                    }
                }
                catch when (cancellationToken.IsCancellationRequested)
                { }
            });
        }
    }
}