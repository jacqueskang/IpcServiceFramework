using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO.Pipes;
using System.Threading;

namespace JKang.IpcServiceFramework.NamedPipe
{
    public class NamedPipeIpcServiceEndpoint : IpcServiceEndpoint
    {
        private readonly ILogger<NamedPipeIpcServiceEndpoint> _logger;
        private readonly NamedPipeOptions _options;

        public NamedPipeIpcServiceEndpoint(string name, IServiceProvider serviceProvider, string pipeName)
            : base(name, serviceProvider)
        {
            PipeName = pipeName;

            _logger = serviceProvider.GetService<ILogger<NamedPipeIpcServiceEndpoint>>();
            _options = serviceProvider.GetRequiredService<NamedPipeOptions>();
        }

        public string PipeName { get; }

        public override void Listen()
        {
            NamedPipeOptions options = ServiceProvider.GetRequiredService<NamedPipeOptions>();

            var threads = new Thread[options.ThreadCount];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(StartServerThread);
                threads[i].Start();
            }

            while (true)
            {
                Thread.Sleep(100);
                for (int i = 0; i < threads.Length; i++)
                {
                    if (threads[i].Join(250))
                    {
                        // thread is finished, starting a new thread
                        threads[i] = new Thread(StartServerThread);
                        threads[i].Start();
                    }
                }
            }
        }

        private void StartServerThread(object obj)
        {
            using (var server = new NamedPipeServerStream(PipeName, PipeDirection.InOut, _options.ThreadCount))
            {
                server.WaitForConnection();

                Process(server, _logger);
            }
        }
    }
}
