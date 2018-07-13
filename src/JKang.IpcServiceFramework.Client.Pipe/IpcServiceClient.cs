using JKang.IpcServiceFramework.IO;
using JKang.IpcServiceFramework.Services;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework
{
    public class PipeIpcServiceClient<TInterface> : IpcServiceClient<TInterface>
        where TInterface : class
    {
        private readonly string _pipeName;

        public PipeIpcServiceClient(string pipeName)
            : this(pipeName, new DefaultIpcMessageSerializer(), new DefaultValueConverter())
        { }

        internal PipeIpcServiceClient(string pipeName,
            IIpcMessageSerializer serializer,
            IValueConverter converter)
            : base(serializer, converter)
        {
            _pipeName = pipeName;
        }

        protected override async Task<IpcResponse> GetResponseAsync(IpcRequest request)
        {
            using (var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.None))
            using (var writer = new IpcWriter(client, Serializer, leaveOpen: true))
            using (var reader = new IpcReader(client, Serializer, leaveOpen: true))
            {
                await client.ConnectAsync();
                return GetIpcResponse(request, reader, writer);
            }
        }
    }
}
