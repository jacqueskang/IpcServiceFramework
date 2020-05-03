using JKang.IpcServiceFramework.Services;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Hosting.Tcp
{
    public class TcpIpcEndpoint<TContract> : IpcEndpoint<TContract>
        where TContract : class
    {
        private readonly TcpIpcEndpointOptions _options;
        private readonly TcpListener _listener;

        public TcpIpcEndpoint(
            TcpIpcEndpointOptions options,
            ILogger<TcpIpcEndpoint<TContract>> logger,
            IServiceProvider serviceProvider)
            : base(options, serviceProvider, logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _listener = new TcpListener(_options.IpEndpoint, _options.Port);
            _listener.Start();
        }

        protected override async Task WaitAndProcessAsync(
            Func<Stream, CancellationToken, Task> process,
            CancellationToken cancellationToken)
        {
            if (process is null)
            {
                throw new ArgumentNullException(nameof(process));
            }

            using (TcpClient client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false))
            {
                Stream server = client.GetStream();

                if (_options.StreamTranslator != null)
                {
                    server = _options.StreamTranslator(server);
                }

                // if SSL is enabled, wrap the stream in an SslStream in client mode
                if (_options.EnableSsl)
                {
                    using (var ssl = new SslStream(server, false))
                    {
                        ssl.AuthenticateAsServer(_options.SslCertificate
                            ?? throw new IpcHostingConfigurationException("Invalid TCP IPC endpoint configured: SSL enabled without providing certificate."));
                        await process(ssl, cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    await process(server, cancellationToken).ConfigureAwait(false);
                }

                client.Close();
            }
        }
    }
}
