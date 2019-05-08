using System;

namespace JKang.IpcServiceFramework
{
    public class IpcRequest
    {
        private Type[] _genericArguments = new Type[0];
        private Type[] _arguments = new Type[0];

        public string MethodName { get; set; }
        public object[] Parameters { get; set; }
        public Type[] ArgumentTypes
        {
            get => _arguments ?? System.Type.EmptyTypes;
            set => _arguments = value;
        }
        public Type[] GenericArguments
        {
            get => _genericArguments ?? new Type[0];
            set => _genericArguments = value;
        }
    }
}