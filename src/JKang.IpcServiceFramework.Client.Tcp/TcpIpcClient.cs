using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Client.Tcp
{
    internal class TcpIpcClient<TInterface> : IpcClient<TInterface>, IDisposable
        where TInterface : class
    {
        private readonly TcpIpcClientOptions _options;
        private TcpClient _client;
        private bool _isDisposed;

        public TcpIpcClient(string name, TcpIpcClientOptions options)
            : base(name, options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        protected override Task<Stream> ConnectToServerAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _client =  new TcpClient();

            if (!_client.ConnectAsync(_options.ServerIp, _options.ServerPort)
                .Wait(_options.ConnectionTimeout, cancellationToken))
            {
                _client.Close();
                cancellationToken.ThrowIfCancellationRequested();
                throw new TimeoutException();
            }

            Stream stream = _client.GetStream();

            // if SSL is enabled, wrap the stream in an SslStream in client mode
            if (_options.EnableSsl)
            {
                SslStream ssl;
                if (_options.SslValidationCallback == null)
                {
                    ssl = new SslStream(stream, false);
                }
                else
                {
                    ssl = new SslStream(stream, false, _options.SslValidationCallback);
                }

                // set client mode and specify the common name(CN) of the server
                if (_options.SslServerIdentity != null)
                {
                    ssl.AuthenticateAsClient(_options.SslServerIdentity);
                }
                stream = ssl;
            }

            return Task.FromResult(stream);
        }

        protected override void EndConnectToServer()
        {
            _client?.Close();
        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                _client?.Dispose();
            }

            _isDisposed = true;
        }
    }
}
