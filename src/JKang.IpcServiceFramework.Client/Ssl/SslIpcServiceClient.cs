using System;
using JKang.IpcServiceFramework.Services;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Security;

namespace JKang.IpcServiceFramework.Ssl
{
    internal class SslIpcServiceClient<TInterface> : IpcServiceClient<TInterface>
        where TInterface : class
    {
        private readonly IPAddress _serverIp;
        private readonly Int32 _serverPort;
        private readonly string _serverName;
        private readonly RemoteCertificateValidationCallback _validationCallback;

        public SslIpcServiceClient(IIpcMessageSerializer serializer, IValueConverter converter, IPAddress serverIp, int serverPort, string serverName)
            : this(serializer, converter, serverIp, serverPort, serverName, null)
        {
        }

        public SslIpcServiceClient(IIpcMessageSerializer serializer, IValueConverter converter, IPAddress serverIp, int serverPort, string serverName, RemoteCertificateValidationCallback validationCallback)
            : base(serializer, converter)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            _serverName = serverName;
            _validationCallback = validationCallback;
        }

        protected override async Task<Stream> ConnectToServerAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var client = new TcpClient();
            IAsyncResult result = client.BeginConnect(_serverIp, _serverPort, null, null);

            await Task.Run(() =>
            {
                // poll every 100ms to check cancellation request
                while (!result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(100), false))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        client.EndConnect(result);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
            });

            cancellationToken.Register(() =>
            {
                client.Close();
            });

            SslStream ssl;
            if (_validationCallback == null)
            {
                ssl = new SslStream(client.GetStream(), false);
            }
            else
            {
                ssl = new SslStream(client.GetStream(), false, _validationCallback);
            }
            ssl.AuthenticateAsClient(_serverName);
            return ssl;
        }
    }
}
