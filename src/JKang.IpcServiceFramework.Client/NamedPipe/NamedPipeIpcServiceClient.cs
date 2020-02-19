using JKang.IpcServiceFramework.Services;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.NamedPipe
{
    internal class NamedPipeIpcServiceClient<TInterface> : IpcServiceClient<TInterface>
        where TInterface : class
    {
        private readonly string _pipeName;
        private readonly Func<Stream, Stream> _streamTranslator;

        public NamedPipeIpcServiceClient(IIpcMessageSerializer serializer, IValueConverter converter, string pipeName)
            : base(serializer, converter)
        {
            _pipeName = pipeName;
        }

        public NamedPipeIpcServiceClient(IIpcMessageSerializer serializer, IValueConverter converter, string pipeName, Func<Stream, Stream> streamTranslator)
            : this(serializer, converter, pipeName)
        {
            _streamTranslator = streamTranslator;
        }

        protected override async Task<Stream> ConnectToServerAsync(CancellationToken cancellationToken)
        {
            var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await client.ConnectAsync(cancellationToken).ConfigureAwait(false);
            return _streamTranslator?.Invoke(client) ?? client;
        }
    }
}
