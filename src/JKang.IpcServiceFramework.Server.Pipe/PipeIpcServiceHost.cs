using JKang.IpcServiceFramework.IO;
using System;
using System.IO.Pipes;

namespace JKang.IpcServiceFramework
{
    public class PipeIpcServiceHost : IpcServiceHost
    {
        private readonly string _pipeName;

        public PipeIpcServiceHost(string pipeName, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _pipeName = pipeName;
        }

        protected override void StartServerThread(object obj)
        {
            using (var server = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, Options.ThreadCount))
            using (var writer = new IpcWriter(server, Serializer, leaveOpen: true))
            using (var reader = new IpcReader(server, Serializer, leaveOpen: true))
            {
                server.WaitForConnection();
                ProcessRequest(reader, writer);
            }
        }
    }
}
