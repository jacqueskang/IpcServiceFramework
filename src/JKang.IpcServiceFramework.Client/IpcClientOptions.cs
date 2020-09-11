using JKang.IpcServiceFramework.Services;
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

        /// <summary>
        /// Indicates the method that will be used during deserialization on the server for locating and loading assemblies.
        /// If <c>false</c>, the assembly used during deserialization must match exactly the assembly used during serialization.
        /// 
        /// If <c>true</c>, the assembly used during deserialization need not match exactly the assembly used during serialization.
        /// Specifically, the version numbers need not match.
        /// 
        /// Default is <c>false</c>.
        /// </summary>
        public bool UseSimpleTypeNameAssemblyFormatHandling { get; set; } = false;

        public IIpcMessageSerializer Serializer { get; set; } = new DefaultIpcMessageSerializer();

        public IValueConverter ValueConverter { get; set; } = new DefaultValueConverter();
    }
}
