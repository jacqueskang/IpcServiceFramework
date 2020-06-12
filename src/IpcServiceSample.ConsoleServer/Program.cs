using IpcServiceSample.ServiceContracts;
using JKang.IpcServiceFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace IpcServiceSample.ConsoleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // build service provider
            IServiceCollection services = ConfigureServices(new ServiceCollection());
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // TODO start IPC service host
            IpcServiceHostBuilder
                .Buid("pipeName", serviceProvider as IServiceProvider)
                .Run();

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
                .AddIpc(options =>
                {
                    options.ThreadCount = 2;
                })
                .AddService<IComputingService, ComputingService>()
                ;

            return services;
        }
    }
}
