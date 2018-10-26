using IpcServiceSample.ServiceContracts;
using JKang.IpcServiceFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IpcServiceSample.ConsoleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // configure DI
            IServiceCollection services = ConfigureServices(new ServiceCollection());

            // build and run service host
            IIpcServiceHost host = new IpcServiceHostBuilder(services.BuildServiceProvider())
                .AddNamedPipeEndpoint<IComputingService>("computingEndpoint", "pipeName")
                .AddTcpEndpoint<ISystemService>("systemEndpoint", IPAddress.Loopback, 45684)
                .Build();

            var source = new CancellationTokenSource();
            Task.WaitAll(host.RunAsync(source.Token), Task.Run(() =>
            {
                Console.WriteLine("Press any key to shutdown.");
                Console.ReadKey();
                source.Cancel();
            }));
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                });

            services
                .AddIpc()
                .AddNamedPipe(options =>
                {
                    options.ThreadCount = 2;
                })
                .AddService<IComputingService, ComputingService>()
                .AddService<ISystemService, SystemService>();

            return services;
        }
    }
}
