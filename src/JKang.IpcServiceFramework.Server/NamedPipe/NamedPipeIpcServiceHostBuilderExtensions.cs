using JKang.IpcServiceFramework.NamedPipe;
using System;
using System.IO;

namespace JKang.IpcServiceFramework
{
    public static class NamedPipeIpcServiceHostBuilderExtensions
    {
        public static IpcServiceHostBuilder AddNamedPipeEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, string pipeName, bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddEndpoint(new NamedPipeIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, pipeName,
                includeFailureDetailsInResponse: includeFailureDetailsInResponse));
        }

        public static IpcServiceHostBuilder AddNamedPipeEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, string pipeName, Func<Stream, Stream> streamTranslator, bool includeFailureDetailsInResponse = false)
            where TContract : class
        {
            return builder.AddEndpoint(new NamedPipeIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, pipeName, streamTranslator,
                includeFailureDetailsInResponse: includeFailureDetailsInResponse));
        }
    }
}
