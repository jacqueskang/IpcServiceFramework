using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.NamedPipe
{
    public class NamedPipeIpcServiceEndpoint<TContract> : IpcServiceEndpoint<TContract>
        where TContract : class
    {
        private readonly ILogger<NamedPipeIpcServiceEndpoint<TContract>> _logger;
        private readonly NamedPipeOptions _options;

        public NamedPipeIpcServiceEndpoint(string name, IServiceProvider serviceProvider, string pipeName)
            : base(name, serviceProvider)
        {
            PipeName = pipeName;

            _logger = serviceProvider.GetService<ILogger<NamedPipeIpcServiceEndpoint<TContract>>>();
            _options = serviceProvider.GetRequiredService<NamedPipeOptions>();
        }

        public string PipeName { get; }

        public override Task ListenAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            NamedPipeOptions options = ServiceProvider.GetRequiredService<NamedPipeOptions>();

            var threads = new Thread[options.ThreadCount];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(StartServerThread);
                threads[i].Start(cancellationToken);
            }

            return Task.Factory.StartNew(() =>
            {
                _logger.LogDebug($"Endpoint '{Name}' listening on pipe '{PipeName}'...");
                while (!cancellationToken.IsCancellationRequested)
                {
                    Thread.Sleep(100);

                    for (int i = 0; i < threads.Length; i++)
                    {
                        if (threads[i].Join(250))
                        {
                            // thread is finished, starting a new thread
                            threads[i] = new Thread(StartServerThread);
                            threads[i].Start(cancellationToken);
                        }
                    }
                }
            });
        }

        private void StartServerThread(object obj)
        {
            var token = (CancellationToken)obj;
            try
            {
                using (var server = new NamedPipeServerStream(PipeName, PipeDirection.InOut, _options.ThreadCount,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                {
                    server.WaitForConnectionAsync(token).Wait();
                    ProcessAsync(server, _logger, token).Wait();
                }
            }
            catch when (token.IsCancellationRequested)
            {
            }
        }
    }
}
