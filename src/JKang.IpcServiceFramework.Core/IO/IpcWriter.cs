using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.IO
{
    public class IpcWriter : IDisposable
    {
        private readonly byte[] _lengthBuffer = new byte[4];
        private readonly Stream _stream;
        private readonly IIpcMessageSerializer _serializer;
        private readonly bool _leaveOpen;

        public IpcWriter(Stream stream, IIpcMessageSerializer serializer)
            : this(stream, serializer, leaveOpen: false)
        { }

        public IpcWriter(Stream stream, IIpcMessageSerializer serializer, bool leaveOpen)
        {
            _stream = stream;
            _serializer = serializer;
            _leaveOpen = leaveOpen;
        }

        public async Task WriteAsync(IpcRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            byte[] binary = _serializer.SerializeRequest(request);
            await WriteMessageAsync(binary, cancellationToken);
        }

        public async Task WriteAsync(IpcResponse response,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            byte[] binary = _serializer.SerializeResponse(response);
            await WriteMessageAsync(binary, cancellationToken);
        }

        private async Task WriteMessageAsync(byte[] binary, CancellationToken cancellationToken)
        {
            int length = binary.Length;
            _lengthBuffer[0] = (byte)length;
            _lengthBuffer[1] = (byte)(length >> 8);
            _lengthBuffer[2] = (byte)(length >> 16);
            _lengthBuffer[3] = (byte)(length >> 24);

            await _stream.WriteAsync(_lengthBuffer, 0, _lengthBuffer.Length, cancellationToken);
            await _stream.WriteAsync(binary, 0, binary.Length, cancellationToken);
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
