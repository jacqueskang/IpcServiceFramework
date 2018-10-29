using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.IO
{
    public class IpcReader : IDisposable
    {
        private readonly byte[] _lengthBuffer = new byte[4];
        private readonly Stream _stream;
        private readonly IIpcMessageSerializer _serializer;
        private readonly bool _leaveOpen;

        public IpcReader(Stream stream, IIpcMessageSerializer serializer)
            : this(stream, serializer, leaveOpen: false)
        { }

        public IpcReader(Stream stream, IIpcMessageSerializer serializer, bool leaveOpen)
        {
            _stream = stream;
            _serializer = serializer;
            _leaveOpen = leaveOpen;
        }

        public async Task<IpcRequest> ReadIpcRequestAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            byte[] binary = await ReadMessageAsync(cancellationToken);
            return _serializer.DeserializeRequest(binary);
        }

        public async Task<IpcResponse> ReadIpcResponseAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            byte[] binary = await ReadMessageAsync(cancellationToken);
            return _serializer.DeserializeResponse(binary);
        }

        private async Task<byte[]> ReadMessageAsync(CancellationToken cancellationToken)
        {
            await _stream.ReadAsync(_lengthBuffer, 0, _lengthBuffer.Length, cancellationToken);
            int length = _lengthBuffer[0] | _lengthBuffer[1] << 8 | _lengthBuffer[2] << 16 | _lengthBuffer[3] << 24;

            byte[] bytes = new byte[length];
            await _stream.ReadAsync(bytes, 0, length, cancellationToken);

            return bytes;
        }

        #region IDisposible

        bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (!_leaveOpen)
                {
                    _stream.Dispose();
                }
            }

            _disposed = true;
        }

        #endregion
    }
}
