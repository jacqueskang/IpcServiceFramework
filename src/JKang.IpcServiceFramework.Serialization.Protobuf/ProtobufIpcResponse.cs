using ProtoBuf;
using System;

namespace JKang.IpcServiceFramework.Serialization.Protobuf
{
    [ProtoContract]
    public class ProtobufIpcResponse
    {
        [ProtoMember(1)]
        public IpcStatus Status { get; set; }

        [ProtoMember(2, DynamicType = true)]
        public object Data { get; set; }

        [ProtoMember(3)]
        public string ErrorMessage { get; set; }

        [ProtoMember(4, DynamicType = true)]
        public Exception InnerException { get; set; }
    }
}
