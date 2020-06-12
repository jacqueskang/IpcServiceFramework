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
            string name, IPAddress ipEndpoint, bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddTcpEndpoint<TContract>(name, ipEndpoint, concurrencyOptions: null,
                includeFailureDetailsInResponse: includeFailureDetailsInResponse);
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, TcpConcurrencyOptions concurrencyOptions,
            bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, concurrencyOptions));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddTcpEndpoint<TContract>(name, ipEndpoint, port, concurrencyOptions: null,
                includeFailureDetailsInResponse: includeFailureDetailsInResponse);
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, TcpConcurrencyOptions concurrencyOptions,
            bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, port, concurrencyOptions,
                includeFailureDetailsInResponse: includeFailureDetailsInResponse));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, Func<Stream, Stream> streamTranslator, bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddTcpEndpoint<TContract>(name, ipEndpoint, streamTranslator, concurrencyOptions: null,
                includeFailureDetailsInResponse: includeFailureDetailsInResponse);
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, Func<Stream, Stream> streamTranslator, TcpConcurrencyOptions concurrencyOptions,
            bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, streamTranslator, concurrencyOptions,
                includeFailureDetailsInResponse: includeFailureDetailsInResponse));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, X509Certificate sslCertificate, bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddTcpEndpoint<TContract>(name, ipEndpoint, sslCertificate, concurrencyOptions: null,
                includeFailureDetailsInResponse: includeFailureDetailsInResponse);
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, X509Certificate sslCertificate, TcpConcurrencyOptions concurrencyOptions,
            bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, sslCertificate, concurrencyOptions,
                includeFailureDetailsInResponse: includeFailureDetailsInResponse));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, X509Certificate sslCertificate, Func<Stream, Stream> streamTranslator,
            bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddTcpEndpoint<TContract>(name, ipEndpoint, sslCertificate, streamTranslator, concurrencyOptions: null,
                includeFailureDetailsInResponse: includeFailureDetailsInResponse);
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, X509Certificate sslCertificate, Func<Stream, Stream> streamTranslator, TcpConcurrencyOptions concurrencyOptions,
            bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, sslCertificate, streamTranslator, concurrencyOptions,
                includeFailureDetailsInResponse: includeFailureDetailsInResponse));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, X509Certificate sslCertificate, bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddTcpEndpoint<TContract>(name, ipEndpoint, port, sslCertificate, concurrencyOptions: null,
                includeFailureDetailsInResponse: includeFailureDetailsInResponse);
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, X509Certificate sslCertificate, TcpConcurrencyOptions concurrencyOptions,
            bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, port, sslCertificate, concurrencyOptions,
                includeFailureDetailsInResponse: includeFailureDetailsInResponse));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, Func<Stream, Stream> streamTranslator, bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddTcpEndpoint<TContract>(name, ipEndpoint, port, streamTranslator, concurrencyOptions: null,
                includeFailureDetailsInResponse: includeFailureDetailsInResponse);
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, Func<Stream, Stream> streamTranslator, TcpConcurrencyOptions concurrencyOptions,
            bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, port, streamTranslator, concurrencyOptions));
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, X509Certificate sslCertificate, Func<Stream, Stream> streamTranslator,
            bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddTcpEndpoint<TContract>(name, ipEndpoint, port, sslCertificate, streamTranslator, concurrencyOptions: null,
                includeFailureDetailsInResponse: includeFailureDetailsInResponse);
        }

        public static IpcServiceHostBuilder AddTcpEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, IPAddress ipEndpoint, int port, X509Certificate sslCertificate, Func<Stream, Stream> streamTranslator, TcpConcurrencyOptions concurrencyOptions,
            bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddEndpoint(new TcpIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, ipEndpoint, port, sslCertificate, streamTranslator, concurrencyOptions,
                includeFailureDetailsInResponse: includeFailureDetailsInResponse));
        }
    }
}
