using JKang.IpcServiceFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace IpcServiceSample.ConsoleServer
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; private set; }

        static void Main(string[] args)
        {
            // build configuration
            IConfigurationBuilder builder = new ConfigurationBuilder();
            Configuration = builder.Build();

            // build service provider
            IServiceCollection services = ConfigureServices(new ServiceCollection());
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // configure console logging
            serviceProvider.GetRequiredService<ILoggerFactory>()
                .AddConsole(LogLevel.Debug);

            // TODO start IPC service host
            IpcServiceHostBuilder
                .Buid("pipeName", serviceProvider as IServiceProvider)
                .Start();

        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            return services
                .AddLogging()
                .AddScoped<IMyIpcService, MyIpcService>()
                ;
        }
    }
}
