﻿using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Hosting.NamedPipe
{
    public class NamedPipeIpcEndpoint<TContract> : IpcEndpoint<TContract>
        where TContract : class
    {
        private readonly NamedPipeIpcEndpointOptions _options;

        public NamedPipeIpcEndpoint(
            NamedPipeIpcEndpointOptions options,
            ILogger<NamedPipeIpcEndpoint<TContract>> logger,
            IServiceProvider serviceProvider)
            : base(options, serviceProvider, logger)
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
