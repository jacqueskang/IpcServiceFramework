using System.Net;
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
    }
}
