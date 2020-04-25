using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IpcServiceSample.ServiceContracts.Helpers
{
    public class LoggingStream : Stream
    {
        private Stream _baseStream;
        private StreamWriter _log;

        public LoggingStream(Stream stream, string logFile)
        {
            _baseStream = stream;
            _log = new StreamWriter(logFile) { AutoFlush = true };
        }

        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => _baseStream.CanSeek;

        public override bool CanWrite => _baseStream.CanWrite;

        public override long Length => _baseStream.Length;

        public override long Position { get => _baseStream.Position; set => _baseStream.Position = value; }

        private enum StreamOperation { Read, Write };
        
        private void Log(StreamOperation direction, byte[] buffer, int offset, int count)
        {
            _log.WriteLine($"[{direction.ToString().ToUpper()}: {count}]");
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

            _log.WriteLine(dump.ToString());
        }

        private void Log(string message)
        {
            _log.WriteLine(message);
        }

        public override void Flush()
        {
            Log("[FLUSH]");
            _baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int br = _baseStream.Read(buffer, offset, count);
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
    }
}
