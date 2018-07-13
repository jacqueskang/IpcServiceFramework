using JKang.IpcServiceFramework.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;

namespace JKang.IpcServiceFramework
{
    public class TcpIpcServiceHost : IpcServiceHost
    {
        private readonly ILogger<TcpIpcServiceHost> _logger;
        private readonly TcpListener _listener;

        public TcpIpcServiceHost(IPAddress ipAddress, int port, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            IpAddress = ipAddress;
            Port = port;
            _logger = serviceProvider.GetService<ILogger<TcpIpcServiceHost>>();
            _listener = new TcpListener(ipAddress, port);
        }

        public IPAddress IpAddress { get; }
        public int Port { get; }

        public override void Run()
        {
            _listener.Start();
            base.Run();
        }

        protected override void StartServerThread(object obj)
        {
            var client = _listener.AcceptTcpClient();

            using (var server = client.GetStream())
            using (var writer = new IpcWriter(server, Serializer, leaveOpen: true))
            using (var reader = new IpcReader(server, Serializer, leaveOpen: true))
            {
                ProcessRequest(reader, writer);
            }
        }
    }
}
