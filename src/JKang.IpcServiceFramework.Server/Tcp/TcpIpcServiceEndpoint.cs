using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace JKang.IpcServiceFramework.Tcp
{
    public class TcpIpcServiceEndpoint : IpcServiceEndpoint
    {
        private readonly ILogger<TcpIpcServiceEndpoint> _logger;
        private readonly TcpListener _listener;

        public TcpIpcServiceEndpoint(String name, IServiceProvider serviceProvider, IPAddress ipEndpoint, int port)
            : base(name, serviceProvider)
        {
            _listener = new TcpListener(ipEndpoint, port);
            _logger = serviceProvider.GetService<ILogger<TcpIpcServiceEndpoint>>();
        }

        public override void Listen()
        {
            _listener.Start();

            _logger?.LogDebug("TCP listener started.");

            while (true)
            {
                var client = _listener.AcceptTcpClient();
                var server = client.GetStream();
                Process(server, _logger);
            }
        }
    }
}