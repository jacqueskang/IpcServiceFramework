using JKang.IpcServiceFramework.NamedPipe;

namespace JKang.IpcServiceFramework
{
    public static class NamedPipeIpcServiceClientBuilderExtensions
    {
        public static IpcServiceClientBuilder<TInterface> UseNamedPipe<TInterface>(
            this IpcServiceClientBuilder<TInterface> builder, string pipeName)
            where TInterface : class
        {
            builder.SetFactory((serializer, converter) => new NamedPipeIpcServiceClient<TInterface>(serializer, converter, pipeName));

            return builder;
        }
    }
}
