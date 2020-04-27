﻿using IpcServiceSample.Server;
using IpcServiceSample.ServiceContracts;
using JKang.IpcServiceFramework.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                .ConfigureServices(services =>
                {
                    services.AddScoped<IInterProcessService, InterProcessService>();
                })
                .ConfigureIpcHost(builder =>
                {
                    builder.AddNamedPipeEndpoint<IInterProcessService>("endpoint1", "pipeinternal");
                });
    }
}
