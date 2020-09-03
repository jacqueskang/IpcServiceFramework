using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Client.NamedPipe
{
    internal class NamedPipeIpcClient<TInterface> : IpcClient<TInterface>
        where TInterface : class
    {
        private readonly NamedPipeIpcClientOptions _options;

        public NamedPipeIpcClient(
            string name,
            NamedPipeIpcClientOptions options)
            : base(name, options)
        {
            _options = options;
        }

        protected override async Task<IpcStreamWrapper> ConnectToServerAsync(CancellationToken cancellationToken)
        {
            var stream = new NamedPipeClientStream(".", _options.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await stream.ConnectAsync(_options.ConnectionTimeout, cancellationToken).ConfigureAwait(false);
            return new IpcStreamWrapper(stream);
        }
    }
}
