using System;

namespace JKang.IpcServiceFramework
{
    public class IpcRequest
    {
        private Type[] _genericArguments = new Type[0];

        public string MethodName { get; set; }
        public object[] Parameters { get; set; }
        public Type[] GenericArguments
        {
            get => _genericArguments ?? new Type[0];
            set => _genericArguments = value;
        }
    }
}