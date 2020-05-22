using JKang.IpcServiceFramework.Hosting;
using System;

namespace Microsoft.Extensions.Hosting
{
    public static class GenericHostBuilderExtensions
    {
        public static IHostBuilder ConfigureIpcHost(this IHostBuilder builder, Action<IIpcHostBuilder> configure)
        {
            var ipcHostBuilder = new IpcHostBuilder(builder);
            configure?.Invoke(ipcHostBuilder);
            return builder;
        }
    }
}
