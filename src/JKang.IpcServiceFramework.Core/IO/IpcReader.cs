using System;
using System.IO;

namespace JKang.IpcServiceFramework.IO
{
    public class IpcReader : IDisposable
    {
        private readonly BinaryReader _reader;
        private readonly IIpcMessageSerializer _serializer;

        public IpcReader(Stream stream, IIpcMessageSerializer serializer)
        {
            _reader = new BinaryReader(stream);
            _serializer = serializer;
        }

        public IpcRequest ReadIpcRequest()
        {
            byte[] binary = ReadMessage();
            return _serializer.DeserializeRequest(binary);
        }

        public IpcResponse ReadIpcResponse()
        {
            byte[] binary = ReadMessage();
            return _serializer.DeserializeResponse(binary);
        }

        private byte[] ReadMessage()
        {
            int length = _reader.ReadInt32();
            return _reader.ReadBytes(length);
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
                _reader.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
