using JKang.IpcServiceFramework.Services;
using System;
using System.IO;
using System.Net;
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
        private readonly TcpClient _client = new TcpClient();
        private bool _isDisposed;

        public TcpIpcClient(
            IIpcMessageSerializer serializer,
            IValueConverter converter,
            IPAddress serverIp,
            int serverPort)
            : this(serializer, converter, new TcpIpcClientOptions
            {
                ServerIp = serverIp ?? throw new ArgumentNullException(nameof(serverIp)),
                ServerPort = serverPort,
            })
        { }

        public TcpIpcClient(
            IIpcMessageSerializer serializer,
            IValueConverter converter,
            TcpIpcClientOptions options)
            : base(serializer, converter)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        protected override async Task<Stream> ConnectToServerAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _client.ConnectAsync(_options.ServerIp, _options.ServerPort).ConfigureAwait(false);

            //IAsyncResult result = _client.BeginConnect(_options.ServerIp, _options.ServerPort, null, null);

            //await Task.Run(() =>
            //{
            //    // poll every 100ms to check cancellation request
            //    while (!result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(100), false))
            //    {
            //        if (cancellationToken.IsCancellationRequested)
            //        {
            //            _client.EndConnect(result);
            //            cancellationToken.ThrowIfCancellationRequested();
            //        }
            //    }
            //}).ConfigureAwait(false);

            //cancellationToken.Register(() =>
            //{
            //    _client.Close();
            //});

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

            return stream;
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
                _client.Dispose();
            }

            _isDisposed = true;
        }
    }
}
