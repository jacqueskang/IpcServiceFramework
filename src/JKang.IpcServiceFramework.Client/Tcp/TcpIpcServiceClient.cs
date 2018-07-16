using System;
using JKang.IpcServiceFramework.Services;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Tcp
{
    internal class TcpIpcServiceClient<TInterface> : IpcServiceClient<TInterface>
        where TInterface : class
    {
        private readonly IPAddress _serverIp;
        private readonly Int32 _serverPort;

        public TcpIpcServiceClient(IIpcMessageSerializer serializer, IValueConverter converter, IPAddress serverIp, int serverPort)
            : base(serializer, converter)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
        }

        protected override async Task<Stream> ConnectToServerAsync()
        {
            var client = new TcpClient();
            await client.ConnectAsync(_serverIp, _serverPort);
            var stream = client.GetStream();

            return stream;
        }
    }
}
