using System.IO;

namespace JKang.IpcServiceFramework.Testing.Fixtures
{
    public class XorStream : Stream
    {
        private readonly Stream _baseStream;

        public XorStream(Stream stream)
        {
            _baseStream = stream;
        }

        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => _baseStream.CanSeek;

        public override bool CanWrite => _baseStream.CanWrite;

        public override long Length => _baseStream.Length;

        public override long Position { get => _baseStream.Position; set => _baseStream.Position = value; }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int br = _baseStream.Read(buffer, offset, count);
            for (int i = offset; i < offset + br; i++)
            {
                buffer[i] ^= 0xFF;
            }

            return br;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] xoredBuffer = new byte[count];
            for (int i = 0; i < count; i++)
            {
                xoredBuffer[i] = (byte)(buffer[offset + i] ^ 0xFF);
            }

            _baseStream.Write(xoredBuffer, 0, count);
        }
    }
}
