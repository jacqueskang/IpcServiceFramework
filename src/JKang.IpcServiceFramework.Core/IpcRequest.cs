using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JKang.IpcServiceFramework
{
    [DataContract]
    public class IpcRequest
    {
        [DataMember]
        public string MethodName { get; set; }

        [DataMember]
        public IEnumerable<object> Parameters { get; set; }

        [DataMember]
        public IEnumerable<Type> ParameterTypes { get; set; }

        [DataMember]
        public IEnumerable<Type> GenericArguments { get; set; }
    }
}
