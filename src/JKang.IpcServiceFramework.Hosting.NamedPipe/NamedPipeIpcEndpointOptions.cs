using JKang.IpcServiceFramework.Hosting.Abstractions;

namespace JKang.IpcServiceFramework.Hosting.NamedPipe
{
    public class NamedPipeIpcEndpointOptions : IpcEndpointOptions
    {
        public string PipeName { get; set; }
    }
}
