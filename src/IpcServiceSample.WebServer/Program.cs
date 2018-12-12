using IpcServiceSample.ServiceContracts;
using JKang.IpcServiceFramework;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace IpcServiceSample.WebServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IWebHost webHost = BuildWebHost(args);

            ThreadPool.QueueUserWorkItem(StartIpcService,
                webHost.Services.CreateScope().ServiceProvider);

            webHost.Run();
        }

        private static void StartIpcService(object state)
        {
            var serviceProvider = state as IServiceProvider;
            new IpcServiceHostBuilder(serviceProvider)
                .AddNamedPipeEndpoint<IComputingService>("computingEndpoint", "pipeName")
                .AddTcpEndpoint<ISystemService>("systemEndpoint", IPAddress.Loopback, 45684)
                .AddSslEndpoint<ISecureService>("secureEndpoint", IPAddress.Loopback, 44384, new X509Certificate2(@"Certificates\server.pfx", "password"))
                .Build()
                .Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
