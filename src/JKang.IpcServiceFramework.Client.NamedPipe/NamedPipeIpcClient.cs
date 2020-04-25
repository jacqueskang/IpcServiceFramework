using JKang.IpcServiceFramework.Services;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Client.NamedPipe
{
    internal class NamedPipeIpcClient<TInterface> : IpcClient<TInterface>
        where TInterface : class
    {
        private readonly string _pipeName;

        public NamedPipeIpcClient(IIpcMessageSerializer serializer, IValueConverter converter, string pipeName)
            : base(serializer, converter)
        {
            _pipeName = pipeName;
        }

        protected override async Task<Stream> ConnectToServerAsync(CancellationToken cancellationToken)
        {
            var stream = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await stream.ConnectAsync(cancellationToken).ConfigureAwait(false);
            return stream;
        }
    }
}
