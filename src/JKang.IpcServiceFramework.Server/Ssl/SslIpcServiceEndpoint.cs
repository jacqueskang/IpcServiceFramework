using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace JKang.IpcServiceFramework.Ssl
{
    public class SslIpcServiceEndpoint<TContract> : IpcServiceEndpoint<TContract>
        where TContract : class
    {
        private readonly ILogger<SslIpcServiceEndpoint<TContract>> _logger;

        public int Port { get; private set; }

        private readonly TcpListener _listener;

        private readonly X509Certificate _certificate;

        public SslIpcServiceEndpoint(String name, IServiceProvider serviceProvider, IPAddress ipEndpoint, X509Certificate certificate)
            : this(name, serviceProvider, ipEndpoint, 443, certificate)
        {
        }

        public SslIpcServiceEndpoint(String name, IServiceProvider serviceProvider, IPAddress ipEndpoint, int port, X509Certificate certificate)
            : base(name, serviceProvider)
        {
            _listener = new TcpListener(ipEndpoint, port);
            _logger = serviceProvider.GetService<ILogger<SslIpcServiceEndpoint<TContract>>>();
            _certificate = certificate;
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
                        SslStream ssl = new SslStream(server);
                        ssl.AuthenticateAsServer(_certificate);
                        await ProcessAsync(ssl, _logger, cancellationToken);
                    }
                }
                catch when (cancellationToken.IsCancellationRequested)
                { }
            });
        }
    }
}