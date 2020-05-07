using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JKang.IpcServiceFramework
{
    [DataContract]
    public class IpcRequest
    {
        [DataMember(Order = 0)]
        public string MethodName { get; set; }

        [DataMember(Order = 10)]
        public IEnumerable<dynamic> Parameters { get; set; }

        [DataMember(Order = 20)]
        public IEnumerable<Type> ParameterTypes { get; set; }

        [DataMember(Order = 30)]
        public IEnumerable<Type> GenericArguments { get; set; }
    }
}
