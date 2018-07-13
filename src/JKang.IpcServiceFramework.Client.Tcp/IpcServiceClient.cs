using JKang.IpcServiceFramework.IO;
using JKang.IpcServiceFramework.Services;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework
{
    public class TcpIpcServiceClient<TInterface> : IpcServiceClient<TInterface>
        where TInterface : class
    {
        private readonly IPAddress _ipAddress;
        private readonly int _port;

        public TcpIpcServiceClient(IPAddress endpoint, int port)
            : this(endpoint, port, new DefaultIpcMessageSerializer(), new DefaultValueConverter())
        { }

        internal TcpIpcServiceClient(IPAddress endpoint,
            int port,
            IIpcMessageSerializer serializer,
            IValueConverter converter)
            : base(serializer, converter)
        {
            _ipAddress = endpoint;
            _port = port;
        }

        protected override async Task<IpcResponse> GetResponseAsync(IpcRequest request)
        {
            var client = new TcpClient();
            await client.ConnectAsync(_ipAddress, _port);

            using (var stream = client.GetStream())
            using (var writer = new IpcWriter(stream, Serializer, leaveOpen: true))
            using (var reader = new IpcReader(stream, Serializer, leaveOpen: true))
            {
                return GetIpcResponse(request, reader, writer);
            }
        }
    }
}
