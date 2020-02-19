using JKang.IpcServiceFramework.NamedPipe;
using System;
using System.IO;

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

        public static IpcServiceClientBuilder<TInterface> UseNamedPipe<TInterface>(
            this IpcServiceClientBuilder<TInterface> builder, string pipeName,
            Func<Stream, Stream> streamTranslator)
            where TInterface : class
        {
            builder.SetFactory((serializer, converter) => new NamedPipeIpcServiceClient<TInterface>(serializer, converter, pipeName, streamTranslator));

            return builder;
        }
    }
}
