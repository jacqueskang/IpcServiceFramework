using JKang.IpcServiceFramework.Hosting.Abstractions;

namespace JKang.IpcServiceFramework.Hosting.NamedPipe
{
    public class NamedPipeIpcServiceEndpointOptions : IpcEndpointOptions
    {
        public NamedPipeIpcServiceEndpointOptions(string pipeName)
        {
            PipeName = pipeName;
        }

        public string PipeName { get; }
    }
}
