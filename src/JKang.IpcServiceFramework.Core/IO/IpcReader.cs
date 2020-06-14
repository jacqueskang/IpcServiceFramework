using JKang.IpcServiceFramework.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.IO
{
    public class IpcReader : IDisposable
    {
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

        /// <exception cref="IpcCommunicationException"></exception>
        /// <exception cref="IpcSerializationException"></exception>
        public async Task<IpcRequest> ReadIpcRequestAsync(CancellationToken cancellationToken = default)
        {
            byte[] binary = await ReadMessageAsync(cancellationToken).ConfigureAwait(false);
            return _serializer.DeserializeRequest(binary);
        }

        /// <exception cref="IpcCommunicationException"></exception>
        /// <exception cref="IpcSerializationException"></exception>
        public async Task<IpcResponse> ReadIpcResponseAsync(CancellationToken cancellationToken = default)
        {
            byte[] binary = await ReadMessageAsync(cancellationToken).ConfigureAwait(false);
            return _serializer.DeserializeResponse(binary);
        }

        private async Task<byte[]> ReadMessageAsync(CancellationToken cancellationToken)
        {
            byte[] lengthBuffer = new byte[4];
            int headerLength = await _stream
                .ReadAsync(lengthBuffer, 0, lengthBuffer.Length, cancellationToken)
                .ConfigureAwait(false);

            if (headerLength != 4)
            {
                throw new IpcCommunicationException($"Invalid message header length must be 4 but was {headerLength}");
            }

            int remainingBytes = lengthBuffer[0] | lengthBuffer[1] << 8 | lengthBuffer[2] << 16 | lengthBuffer[3] << 24;

            byte[] buffer = new byte[65536];
            int offset = 0;

            using (var ms = new MemoryStream())
            {
                while (remainingBytes > 0)
                {
                    int count = Math.Min(buffer.Length, remainingBytes);
                    int actualCount = await _stream
                        .ReadAsync(buffer, offset, count, cancellationToken)
                        .ConfigureAwait(false);

                    if (actualCount == 0)
                    {
                        throw new IpcCommunicationException("Stream closed unexpectedly.");
                    }

                    ms.Write(buffer, 0, actualCount);
                    remainingBytes -= actualCount;
                }
                return ms.ToArray();
            }
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
