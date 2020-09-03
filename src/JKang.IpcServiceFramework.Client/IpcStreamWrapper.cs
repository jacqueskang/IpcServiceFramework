using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace JKang.IpcServiceFramework.Client
{
    /// <summary>
    /// Tcp clients depend on both a TcpClient object and a Stream object.
    /// This wrapper class ensures they are simultaneously disposed of.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class IpcStreamWrapper : IDisposable
    {
        private IDisposable _context;
        bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="IpcStreamWrapper"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="context">The IDisposable context the Stream depends on.</param>
        public IpcStreamWrapper(Stream stream, IDisposable context = null)
        {
            Stream = stream;
            _context = context;
        }

        public Stream Stream { get; private set; }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                Stream?.Dispose();
                _context?.Dispose();
            }
        }
    }
}
