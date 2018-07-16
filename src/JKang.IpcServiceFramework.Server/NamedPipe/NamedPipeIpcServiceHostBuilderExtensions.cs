using JKang.IpcServiceFramework.NamedPipe;

namespace JKang.IpcServiceFramework
{
    public static class NamedPipeIpcServiceHostBuilderExtensions
    {
        public static IpcServiceHostBuilder AddNamedPipeEndpoint(this IpcServiceHostBuilder builder,
            string name, string pipeName)
        {
            return builder.AddEndpoint(new NamedPipeIpcServiceEndpoint(name, builder.ServiceProvider, pipeName));
        }
    }
}
