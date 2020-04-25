using IpcServiceSample.ServiceContracts;
using IpcServiceSample.ServiceContracts.Helpers;
using JKang.IpcServiceFramework;
using JKang.IpcServiceFramework.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace IpcServiceSample.ConsoleServer
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    services
                        .AddScoped<IComputingService, ComputingService>()
                        .AddScoped<ISystemService, SystemService>()
                        .AddScoped<ITestService, TestService>()
                        ;
                })
                .ConfigureIpcHost(builder =>
                {
                    builder
                        .AddNamedPipeEndpoint<IComputingService>("endpoint1", "pipeName")
                        .AddNamedPipeEndpoint<IComputingService>("endpoint2", "pipeName2")
                        ;
                })
                .ConfigureLogging(builder =>
                {
                    builder
                        .AddConsole()
                        .SetMinimumLevel(LogLevel.Debug);
                })
                ;

        //static void Main(string[] args)
        //{
        //    // configure DI
        //    IServiceCollection services = ConfigureServices(new ServiceCollection());

        //    // build and run service host
        //    IIpcServiceHost host = new IpcServiceHostBuilder(services.BuildServiceProvider())
        //        .AddNamedPipeEndpoint<IComputingService>("computingEndpoint", "pipeName")
        //        .AddTcpEndpoint<ISystemService>("systemEndpoint", IPAddress.Loopback, 45684)
        //        .AddTcpEndpoint<ITestService>("secureEndpoint", IPAddress.Loopback, 44384, new X509Certificate2(@"Certificates\server.pfx", "password"))
        //        .AddTcpEndpoint<ITestService>("xorTranslatedEndpoint", IPAddress.Loopback, 45454, s => new XorStream(s))
        //        .Build();

        //    var source = new CancellationTokenSource();
        //    Task.WaitAll(host.RunAsync(source.Token), Task.Run(() =>
        //    {
        //        Console.WriteLine("Press any key to shutdown.");
        //        Console.ReadKey();
        //        source.Cancel();
        //    }));

        //    Console.WriteLine("Server stopped.");
        //}

        //private static IServiceCollection ConfigureServices(IServiceCollection services)
        //{
        //    return services
        //        .AddLogging(builder =>
        //        {
        //            builder.AddConsole();
        //            builder.SetMinimumLevel(LogLevel.Debug);
        //        })
        //        .AddIpc(builder =>
        //        {
        //            builder
        //                .AddNamedPipe(options =>
        //                {
        //                    options.ThreadCount = 2;
        //                })
        //                .AddService<IComputingService, ComputingService>()
        //                .AddService<ISystemService, SystemService>()
        //                .AddService<ITestService, TestService>();
        //        });
        //}
    }
}
