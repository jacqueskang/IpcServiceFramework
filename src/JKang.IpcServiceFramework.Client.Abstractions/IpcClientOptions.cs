using System;
using System.IO;

namespace JKang.IpcServiceFramework.Client
{
    public class IpcClientOptions
    {
        public Func<Stream, Stream> StreamTranslator { get; set; }
    }
}
