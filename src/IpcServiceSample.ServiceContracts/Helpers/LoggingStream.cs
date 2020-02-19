using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IpcServiceSample.ServiceContracts.Helpers
{
    public class LoggingStream : Stream
    {
        private Stream _baseStream;
        private StreamWriter _log;
        private readonly ReaderWriterLockSlim _logLock;

        private static readonly ConcurrentDictionary<string, ReaderWriterLockSlim> _readWriteLocks = new ConcurrentDictionary<string, ReaderWriterLockSlim>();

        public LoggingStream(Stream stream, string logFile)
        {
            _baseStream = stream;

            var fs = File.Open(logFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            _log = new StreamWriter(fs) { AutoFlush = true };

            _logLock = _readWriteLocks.GetOrAdd(logFile, new ReaderWriterLockSlim());
        }

        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => _baseStream.CanSeek;

        public override bool CanWrite => _baseStream.CanWrite;

        public override long Length => _baseStream.Length;

        public override long Position { get => _baseStream.Position; set => _baseStream.Position = value; }

        private enum StreamOperation { Read, Write };

        private void Log(StreamOperation direction, byte[] buffer, int offset, int count)
        {
            var dump = new StringBuilder();
            var hex = new StringBuilder();
            var ascii = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                if (i > 0 && i % 16 == 0)
                {
                    dump.Append(hex.ToString().PadRight(48, ' '));
                    dump.Append(' ', 3);
                    dump.Append(ascii);
                    dump.AppendLine();
                    hex.Clear();
                    ascii.Clear();
                }

                byte c = buffer[offset + i];

                hex.AppendFormat("{0:x2} ", c);

                if (c >= 32 && c < 255)
                    ascii.Append((char)c);
                else
                    ascii.Append('.');
            }
            if (ascii.Length > 0)
            {
                dump.Append(hex.ToString().PadRight(48, ' '));
                dump.Append(' ', 3);
                dump.Append(ascii);
                dump.AppendLine();
            }

            try
            {
                _logLock.EnterWriteLock();
                _log.WriteLine($"[{direction.ToString().ToUpper()}: {count}]");
                _log.WriteLine(dump.ToString());
            }
            finally
            {
                _logLock.ExitWriteLock();
            }
        }

        private void Log(string message)
        {
            try
            {
                _logLock.EnterWriteLock();
                _log.WriteLine(message);
            }
            finally
            {
                _logLock.ExitWriteLock();
            }
        }

        public override void Flush()
        {
            Log("[FLUSH]");
            _baseStream.Flush();
        }

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            Log("[FLUSH]");
            await _baseStream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int br = _baseStream.Read(buffer, offset, count);
            Log(StreamOperation.Read, buffer, offset, br);
            return br;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int br = await _baseStream.ReadAsync(buffer, offset, count, cancellationToken);
            Log(StreamOperation.Read, buffer, offset, br);
            return br;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            Log($"[SEEK: {origin.ToString()}+{offset}]");
            return _baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            Log($"[SET LENGTH: {value}]");
            _baseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _baseStream.Write(buffer, offset, count);
            Log(StreamOperation.Write, buffer, offset, count);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _baseStream.WriteAsync(buffer, offset, count, cancellationToken);
            Log(StreamOperation.Write, buffer, offset, count);
        }
    }
}
