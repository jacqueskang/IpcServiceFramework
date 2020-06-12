using System;
using System.IO;
using System.Net;
using System.Net.Security;
using JKang.IpcServiceFramework.Tcp;

namespace JKang.IpcServiceFramework
{
    public static class TcpIpcServiceClientBuilderExtensions
    {
        public static IpcServiceClientBuilder<TInterface> UseTcp<TInterface>(
            this IpcServiceClientBuilder<TInterface> builder, IPAddress serverIp, int serverPort)
            where TInterface : class
        {
            builder.SetFactory((serializer, converter) => new TcpIpcServiceClient<TInterface>(serializer, converter, serverIp, serverPort));

            return builder;
        }

        public static IpcServiceClientBuilder<TInterface> UseTcp<TInterface>(
            this IpcServiceClientBuilder<TInterface> builder, IPAddress serverIp, int serverPort,
            Func<Stream, Stream> streamTranslator)
            where TInterface : class
        {
            builder.SetFactory((serializer, converter) => new TcpIpcServiceClient<TInterface>(serializer, converter, serverIp, serverPort, streamTranslator));

            return builder;
        }

        public static IpcServiceClientBuilder<TInterface> UseTcp<TInterface>(
            this IpcServiceClientBuilder<TInterface> builder, IPAddress serverIp, int serverPort,
            string sslServerIdentity)
            where TInterface : class
        {
            builder.SetFactory((serializer, converter) => new TcpIpcServiceClient<TInterface>(serializer, converter, serverIp, serverPort, sslServerIdentity));

            return builder;
        }

        public static IpcServiceClientBuilder<TInterface> UseTcp<TInterface>(
           this IpcServiceClientBuilder<TInterface> builder, IPAddress serverIp, int serverPort,
           string sslServerIdentity, RemoteCertificateValidationCallback sslCertificateValidationCallback)
           where TInterface : class
        {
            builder.SetFactory((serializer, converter) => new TcpIpcServiceClient<TInterface>(serializer, converter, serverIp, serverPort, sslServerIdentity, sslCertificateValidationCallback));

            return builder;
        }

        public static IpcServiceClientBuilder<TInterface> UseTcp<TInterface>(
            this IpcServiceClientBuilder<TInterface> builder, IPAddress serverIp, int serverPort,
            string sslServerIdentity, Func<Stream, Stream> streamTranslator)
            where TInterface : class
        {
            builder.SetFactory((serializer, converter) => new TcpIpcServiceClient<TInterface>(serializer, converter, serverIp, serverPort, sslServerIdentity, streamTranslator));

            return builder;
        }

        public static IpcServiceClientBuilder<TInterface> UseTcp<TInterface>(
            this IpcServiceClientBuilder<TInterface> builder, IPAddress serverIp, int serverPort,
            string sslServerIdentity, RemoteCertificateValidationCallback sslCertificateValidationCallback, Func<Stream, Stream> streamTranslator)
            where TInterface : class
        {
            builder.SetFactory((serializer, converter) => new TcpIpcServiceClient<TInterface>(serializer, converter, serverIp, serverPort, sslServerIdentity, sslCertificateValidationCallback, streamTranslator));

            return builder;
        }
    }
}
