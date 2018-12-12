using System.Net;
using System.Net.Security;
using JKang.IpcServiceFramework.Ssl;

namespace JKang.IpcServiceFramework
{
    public static class SslIpcServiceClientBuilderExtensions
    {
        public static IpcServiceClientBuilder<TInterface> UseSsl<TInterface>(
            this IpcServiceClientBuilder<TInterface> builder, IPAddress serverIp, int serverPort, string serverName)
            where TInterface : class
        {
            builder.SetFactory((serializer, converter) => new SslIpcServiceClient<TInterface>(serializer, converter, serverIp, serverPort, serverName));

            return builder;
        }

        public static IpcServiceClientBuilder<TInterface> UseSsl<TInterface>(
            this IpcServiceClientBuilder<TInterface> builder, IPAddress serverIp, int serverPort, string serverName, RemoteCertificateValidationCallback validationCallback)
            where TInterface : class
        {
            builder.SetFactory((serializer, converter) => new SslIpcServiceClient<TInterface>(serializer, converter, serverIp, serverPort, serverName, validationCallback));

            return builder;
        }
    }
}
