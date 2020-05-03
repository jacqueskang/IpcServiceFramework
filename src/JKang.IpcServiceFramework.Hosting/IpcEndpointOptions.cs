using JKang.IpcServiceFramework.Services;
using System;
using System.IO;

namespace JKang.IpcServiceFramework.Hosting
{
    public class IpcEndpointOptions
    {
        public int MaxConcurrentCalls { get; set; } = 4;

        public bool IncludeFailureDetailsInResponse { get; set; }

        public Func<Stream, Stream> StreamTranslator { get; set; }

        public IIpcMessageSerializer Serializer { get; set; } = new DefaultIpcMessageSerializer();

        public IValueConverter ValueConverter { get; set; } = new DefaultValueConverter();
    }
}
