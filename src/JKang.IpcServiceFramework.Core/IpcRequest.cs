using System;

namespace JKang.IpcServiceFramework
{
    public class IpcRequest
    {
        public string MethodName { get; set; }
        public object[] Parameters { get; set; }
        public Type[] GenericArguments { get; set; }
    }
}