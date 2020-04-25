using JKang.IpcServiceFramework.Services;
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

        public async Task<IpcRequest> ReadIpcRequestAsync(CancellationToken cancellationToken = default)
        {
            byte[] binary = await ReadMessageAsync(cancellationToken).ConfigureAwait(false);
            return _serializer.DeserializeRequest(binary);
        }

        public async Task<IpcResponse> ReadIpcResponseAsync(CancellationToken cancellationToken = default)
        {
            byte[] binary = await ReadMessageAsync(cancellationToken).ConfigureAwait(false);
            return _serializer.DeserializeResponse(binary);
        }

        private async Task<byte[]> ReadMessageAsync(CancellationToken cancellationToken)
        {
            int headerLength = await _stream
                .ReadAsync(_lengthBuffer, 0, _lengthBuffer.Length, cancellationToken)
                .ConfigureAwait(false);

            if (headerLength != 4)
            {
                throw new ArgumentOutOfRangeException($"Header length must be 4 but was {headerLength}");
            }

            int expectedLength = _lengthBuffer[0] | _lengthBuffer[1] << 8 | _lengthBuffer[2] << 16 | _lengthBuffer[3] << 24;
            byte[] bytes = new byte[expectedLength];
            int totalBytesReceived = 0;
            int remainingBytes = expectedLength;

            using (var ms = new MemoryStream())
            {
                while (totalBytesReceived < expectedLength)
                {
                    int dataLength = await _stream
                        .ReadAsync(bytes, 0, remainingBytes, cancellationToken)
                        .ConfigureAwait(false);

                    if (dataLength == 0)
                    {
                        break;             // end of stream or stream shut down.
                    }

                    ms.Write(bytes, 0, dataLength);
                    totalBytesReceived += dataLength;
                    remainingBytes -= dataLength;
                }

                bytes = ms.ToArray();
            }

            if (totalBytesReceived != expectedLength)
            {
                throw new System.ArgumentOutOfRangeException($"Data length must be {expectedLength} but was {totalBytesReceived}");
            }

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
