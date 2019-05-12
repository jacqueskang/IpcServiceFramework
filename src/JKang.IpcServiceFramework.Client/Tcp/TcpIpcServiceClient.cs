using System;
using JKang.IpcServiceFramework.Services;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Security;

namespace JKang.IpcServiceFramework.Tcp
{
    internal class TcpIpcServiceClient<TInterface> : IpcServiceClient<TInterface>
        where TInterface : class
    {
        private readonly IPAddress _serverIp;
        private readonly Int32 _serverPort;
        private readonly Func<Stream, Stream> _streamTranslator;
        private readonly string _sslServerIdentity;
        private readonly RemoteCertificateValidationCallback _sslValidationCallback;


        public bool SSL { get; private set; }

        public TcpIpcServiceClient(IIpcMessageSerializer serializer, IValueConverter converter, IPAddress serverIp, int serverPort)
            : base(serializer, converter)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            SSL = false;
        }

        public TcpIpcServiceClient(IIpcMessageSerializer serializer, IValueConverter converter, IPAddress serverIp, int serverPort, Func<Stream, Stream> streamTranslator)
            : this(serializer, converter, serverIp, serverPort)
        {
            _streamTranslator = streamTranslator;
        }

        public TcpIpcServiceClient(IIpcMessageSerializer serializer, IValueConverter converter, IPAddress serverIp, int serverPort, string sslServerIdentity, RemoteCertificateValidationCallback sslCertificateValidationCallback)
            : this(serializer, converter, serverIp, serverPort)
        {
            _sslValidationCallback = sslCertificateValidationCallback;
            _sslServerIdentity = sslServerIdentity;
            SSL = true;
        }

        public TcpIpcServiceClient(IIpcMessageSerializer serializer, IValueConverter converter, IPAddress serverIp, int serverPort, string sslServerIdentity)
            : this(serializer, converter, serverIp, serverPort, sslServerIdentity, (RemoteCertificateValidationCallback)null)
        {
        }

        public TcpIpcServiceClient(IIpcMessageSerializer serializer, IValueConverter converter, IPAddress serverIp, int serverPort, string sslServerIdentity, Func<Stream, Stream> streamTranslator)
            : this(serializer, converter, serverIp, serverPort, sslServerIdentity, (RemoteCertificateValidationCallback)null)
        {
            _streamTranslator = streamTranslator;
        }

        public TcpIpcServiceClient(IIpcMessageSerializer serializer, IValueConverter converter, IPAddress serverIp, int serverPort, string sslServerIdentity, RemoteCertificateValidationCallback sslCertificateValidationCallback, Func<Stream, Stream> streamTranslator)
            : this(serializer, converter, serverIp, serverPort, sslServerIdentity, sslCertificateValidationCallback)
        {
            _streamTranslator = streamTranslator;
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
            }).ConfigureAwait(false);

            cancellationToken.Register(() =>
            {
                client.Close();
            });

            Stream stream = client.GetStream();

            // if there's a stream translator, apply it here
            if (_streamTranslator != null)
            {
                stream = _streamTranslator(stream);
            }

            // if SSL is enabled, wrap the stream in an SslStream in client mode
            if (SSL)
            {
                SslStream ssl;
                if (_sslValidationCallback == null)
                {
                    ssl = new SslStream(stream, false);
                }
                else
                {
                    ssl = new SslStream(stream, false, _sslValidationCallback);
                }

                // set client mode and specify the common name(CN) of the server
                ssl.AuthenticateAsClient(_sslServerIdentity);
                stream = ssl;
            }

            return stream;
        }
    }
}
