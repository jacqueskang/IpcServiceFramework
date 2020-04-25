using System;

namespace JKang.IpcServiceFramework
{
    public class IpcRequest
    {
        private Type[] _genericArguments = new Type[0];

        public string MethodName { get; set; }
        public object[] Parameters { get; set; }

        /// <summary>
        /// Gets or sets the types of parameter of the IPC method to call
        /// </summary>
        public Type[] ParameterTypes { get; set; } = new Type[0];

        public Type[] GenericArguments
        {
            get => _genericArguments ?? new Type[0];
            set => _genericArguments = value;
        }
    }
}
