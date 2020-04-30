using System;
using System.IO;

namespace JKang.IpcServiceFramework.Client
{
    public class IpcClientOptions
    {
        public Func<Stream, Stream> StreamTranslator { get; set; }

        /// <summary>
        /// The number of milliseconds to wait for the server to respond before
        /// the connection times out. Default value is 60000.
        /// </summary>
        public int ConnectionTimeout { get; set; } = 60000;
    }
}
