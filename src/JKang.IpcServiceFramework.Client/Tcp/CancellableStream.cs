using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Tcp
{
    internal class CancellableStream : Stream
    {
        Stream _wrapped;
        CancellationToken _cancellationToken;
        CancellationTokenRegistration _cancellationRegistration;

        public CancellableStream(Stream toWrap, CancellationToken cancellationToken)
        {
            _wrapped = toWrap;
            _cancellationToken = cancellationToken;
            _cancellationRegistration = cancellationToken.Register(() => _wrapped.Dispose());
        }

        protected override void Dispose(bool disposing)
        {
            _cancellationRegistration.Dispose();
            _cancellationRegistration = default(CancellationTokenRegistration);

            _cancellationToken = CancellationToken.None;

            base.Dispose(disposing);
        }

        void WrapOperation(Action action)
        {
            try
            {
                action();
            }
            catch when (_cancellationToken.IsCancellationRequested)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                throw;
            }
        }

        T WrapOperation<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch when (_cancellationToken.IsCancellationRequested)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                throw;
            }
        }

        async Task WrapOperationAsync(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch when (_cancellationToken.IsCancellationRequested)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                throw;
            }
        }

        async Task<T> WrapOperationAsync<T>(Func<Task<T>> action)
        {
            try
            {
                return await action();
            }
            catch when (_cancellationToken.IsCancellationRequested)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                throw;
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => WrapOperation(() => _wrapped.BeginRead(buffer, offset, count, callback, state));
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => WrapOperation(() => _wrapped.BeginWrite(buffer, offset, count, callback, state));
        public override bool CanRead
            => WrapOperation(() => _wrapped.CanRead);
        public override bool CanSeek
            => WrapOperation(() => _wrapped.CanSeek);
        public override bool CanTimeout
            => WrapOperation(() => _wrapped.CanTimeout);
        public override bool CanWrite
            => WrapOperation(() => _wrapped.CanWrite);
        public override void Close()
            => WrapOperation(() => _wrapped.Close());
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            => WrapOperationAsync(async () => await _wrapped.CopyToAsync(destination, bufferSize, cancellationToken));
        public override int EndRead(IAsyncResult asyncResult)
            => WrapOperation(() => _wrapped.EndRead(asyncResult));
        public override void EndWrite(IAsyncResult asyncResult)
            => WrapOperation(() => _wrapped.EndWrite(asyncResult));
        public override bool Equals(object obj)
            => WrapOperation(() => _wrapped.Equals(obj));
        public override void Flush()
            => WrapOperation(() => _wrapped.Flush());
        public override Task FlushAsync(CancellationToken cancellationToken)
            => WrapOperationAsync(async () => await _wrapped.FlushAsync(cancellationToken));
        public override int GetHashCode()
            => WrapOperation(() => _wrapped.GetHashCode());
        public override object InitializeLifetimeService()
            => WrapOperation(() => _wrapped.InitializeLifetimeService());
        public override long Length
            => WrapOperation(() => _wrapped.Length);
        public override long Position
        {
            get => WrapOperation(() => _wrapped.Position);
            set => WrapOperation(() => _wrapped.Position = value);
        }
        public override int Read(byte[] buffer, int offset, int count)
            => WrapOperation(() => _wrapped.Read(buffer, offset, count));
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => WrapOperationAsync(() => _wrapped.ReadAsync(buffer, offset, count, cancellationToken));
        public override int ReadByte()
            => WrapOperation(() => _wrapped.ReadByte());
        public override int ReadTimeout
        {
            get => WrapOperation(() => _wrapped.ReadTimeout);
            set => WrapOperation(() => _wrapped.ReadTimeout = value);
        }
        public override long Seek(long offset, SeekOrigin origin)
            => WrapOperation(() => _wrapped.Seek(offset, origin));
        public override void SetLength(long value)
            => WrapOperation(() => _wrapped.SetLength(value));
        public override string ToString()
            => WrapOperation(() => _wrapped.ToString());
        public override void Write(byte[] buffer, int offset, int count)
            => WrapOperation(() => _wrapped.Write(buffer, offset, count));
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => WrapOperation(() => _wrapped.WriteAsync(buffer, offset, count, cancellationToken));
        public override void WriteByte(byte value)
            => WrapOperation(() => _wrapped.WriteByte(value));
        public override int WriteTimeout
        {
            get => WrapOperation(() => _wrapped.WriteTimeout);
            set => WrapOperation(() => _wrapped.WriteTimeout = value);
        }
    }
}
