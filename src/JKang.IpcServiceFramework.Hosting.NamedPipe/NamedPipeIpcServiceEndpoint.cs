using JKang.IpcServiceFramework.Services;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Hosting.NamedPipe
{
    public class NamedPipeIpcServiceEndpoint<TContract> : IpcEndpoint<TContract>
        where TContract : class
    {
        private readonly NamedPipeIpcServiceEndpointOptions _options;

        public NamedPipeIpcServiceEndpoint(
            string name,
            NamedPipeIpcServiceEndpointOptions options,
            IIpcMessageSerializer serializer,
            IValueConverter valueConverter,
            ILogger<NamedPipeIpcServiceEndpoint<TContract>> logger,
            IServiceProvider serviceProvider)
            : base(name, options, serviceProvider, serializer, valueConverter, logger)
        {
            _options = options;
        }

        protected override async Task WaitAndProcessAsync(
            Func<Stream, CancellationToken, Task> process,
            CancellationToken cancellationToken)
        {
            if (process is null)
            {
                throw new ArgumentNullException(nameof(process));
            }

            using (var server = new NamedPipeServerStream(_options.PipeName, PipeDirection.InOut, _options.MaxConcurrentCalls,
                PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                await process(server, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
