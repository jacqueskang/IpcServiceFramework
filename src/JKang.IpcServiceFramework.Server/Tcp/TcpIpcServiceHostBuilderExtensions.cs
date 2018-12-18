using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using JKang.IpcServiceFramework.Tcp;

namespace JKang.IpcServiceFramework
{
    public static class TcpIpcServiceHostBuilderExtensions
    {
        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, port));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, Func<Stream, Stream> streamTranslator)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, streamTranslator));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, X509Certificate sslCertificate)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, sslCertificate));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, X509Certificate sslCertificate, Func<Stream, Stream> streamTranslator)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, sslCertificate, streamTranslator));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, X509Certificate sslCertificate)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, port, sslCertificate));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, Func<Stream, Stream> streamTranslator)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, port, streamTranslator));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, X509Certificate sslCertificate, Func<Stream, Stream> streamTranslator)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, port, sslCertificate, streamTranslator));
        }
    }
}
