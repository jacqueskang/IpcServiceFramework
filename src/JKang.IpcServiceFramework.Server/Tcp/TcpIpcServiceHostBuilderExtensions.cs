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
            return builder.AddTcpEndpoint<TContract>(name, ipEndpoint, concurrencyOptions: null);
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, TcpConcurrencyOptions concurrencyOptions)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, concurrencyOptions));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port)
            where TContract : class
        {
            return builder.AddTcpEndpoint<TContract>(name, ipEndpoint, port, concurrencyOptions: null);
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, TcpConcurrencyOptions concurrencyOptions)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, port, concurrencyOptions));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, Func<Stream, Stream> streamTranslator)
            where TContract : class
        {
            return builder.AddTcpEndpoint<TContract>(name, ipEndpoint, streamTranslator, concurrencyOptions: null);
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, Func<Stream, Stream> streamTranslator, TcpConcurrencyOptions concurrencyOptions)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, streamTranslator, concurrencyOptions));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, X509Certificate sslCertificate)
            where TContract : class
        {
            return builder.AddTcpEndpoint<TContract>(name, ipEndpoint, sslCertificate, concurrencyOptions: null);
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, X509Certificate sslCertificate, TcpConcurrencyOptions concurrencyOptions)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, sslCertificate, concurrencyOptions));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, X509Certificate sslCertificate, Func<Stream, Stream> streamTranslator)
            where TContract : class
        {
            return builder.AddTcpEndpoint<TContract>(name, ipEndpoint, sslCertificate, streamTranslator, concurrencyOptions: null);
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, X509Certificate sslCertificate, Func<Stream, Stream> streamTranslator, TcpConcurrencyOptions concurrencyOptions)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, sslCertificate, streamTranslator, concurrencyOptions));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, X509Certificate sslCertificate)
            where TContract : class
        {
            return builder.AddTcpEndpoint<TContract>(name, ipEndpoint, port, sslCertificate, concurrencyOptions: null);
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, X509Certificate sslCertificate, TcpConcurrencyOptions concurrencyOptions)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, port, sslCertificate, concurrencyOptions));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, Func<Stream, Stream> streamTranslator)
            where TContract : class
        {
            return builder.AddTcpEndpoint<TContract>(name, ipEndpoint, port, streamTranslator, concurrencyOptions: null);
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, Func<Stream, Stream> streamTranslator, TcpConcurrencyOptions concurrencyOptions)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, port, streamTranslator, concurrencyOptions));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, X509Certificate sslCertificate, Func<Stream, Stream> streamTranslator)
            where TContract : class
        {
            return builder.AddTcpEndpoint<TContract>(name, ipEndpoint, port, sslCertificate, streamTranslator, concurrencyOptions: null);
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, X509Certificate sslCertificate, Func<Stream, Stream> streamTranslator, TcpConcurrencyOptions concurrencyOptions)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, port, sslCertificate, streamTranslator, concurrencyOptions));
        }
    }
}
