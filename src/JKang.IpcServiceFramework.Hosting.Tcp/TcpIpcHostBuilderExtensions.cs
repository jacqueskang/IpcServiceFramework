﻿using JKang.IpcServiceFramework.Hosting.Tcp;
using JKang.IpcServiceFramework.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace JKang.IpcServiceFramework.Hosting
{
    public static class TcpIpcHostBuilderExtensions
    {
        public static IIpcHostBuilder AddTcpEndpoint<TContract>(this IIpcHostBuilder builder,
            IPAddress ipEndpoint, int port)
            where TContract : class
        {
            return builder.AddTcpEndpoint<TContract>(options =>
            {
                options.IpEndpoint = ipEndpoint;
                options.Port = port;
            });
        }

        public static IIpcHostBuilder AddTcpEndpoint<TContract>(this IIpcHostBuilder builder,
            Action<TcpIpcEndpointOptions> configure)
            where TContract : class
        {
            var options = new TcpIpcEndpointOptions();
            configure?.Invoke(options);

            builder.AddIpcEndpoint(serviceProvider =>
            {
                IIpcMessageSerializer serializer = serviceProvider.GetRequiredService<IIpcMessageSerializer>();
                IValueConverter valueConverter = serviceProvider.GetRequiredService<IValueConverter>();
                ILogger<TcpIpcEndpoint<TContract>> logger = serviceProvider
                    .GetRequiredService<ILogger<TcpIpcEndpoint<TContract>>>();

                return new TcpIpcEndpoint<TContract>(options, serializer, valueConverter, logger, serviceProvider);
            });

            return builder;
        }
    }
}
