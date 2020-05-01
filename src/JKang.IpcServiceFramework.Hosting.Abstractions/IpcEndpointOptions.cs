using System;
using System.IO;

namespace JKang.IpcServiceFramework.Hosting.Abstractions
{
    public class IpcEndpointOptions
    {
        public int MaxConcurrentCalls { get; set; } = 4;

        public bool IncludeFailureDetailsInResponse { get; set; }

        public Func<Stream, Stream> StreamTranslator { get; set; }
    }
}
