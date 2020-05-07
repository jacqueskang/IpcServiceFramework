using ProtoBuf;
using System;
using System.Collections.Generic;

namespace JKang.IpcServiceFramework.Serialization.Protobuf
{
    [ProtoContract]
    public class ProtobufIpcRequest
    {
        [ProtoMember(1)]
        public string MethodName { get; set; }

        [ProtoMember(2, DynamicType = true)]
        public IEnumerable<object> Parameters { get; set; }

        [ProtoMember(3)]
        public IEnumerable<Type> ParameterTypes { get; set; }

        [ProtoMember(4)]
        public IEnumerable<Type> GenericArguments { get; set; }
    }
}
