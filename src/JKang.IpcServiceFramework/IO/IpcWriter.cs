using System;
using System.IO;

namespace JKang.IpcServiceFramework.IO
{
    public class IpcWriter: IDisposable
    {
        private readonly BinaryWriter _writer;
        private readonly IIpcMessageSerializer _serializer;

        public IpcWriter(Stream stream, IIpcMessageSerializer serializer)
        {
            _writer = new BinaryWriter(stream);
            _serializer = serializer;
        }

        public void Write(IpcRequest request)
        {
            byte[] binary = _serializer.SerializeRequest(request);
            WriteMessage(binary);
        }

        public void Write(IpcResponse response)
        {
            byte[] binary = _serializer.SerializeResponse(response);
            WriteMessage(binary);
        }

        private void WriteMessage(byte[] binary)
        {
            _writer.Write(binary.Length);
            _writer.Write(binary);
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
                _writer.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
