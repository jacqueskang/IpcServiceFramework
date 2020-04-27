using System;

namespace JKang.IpcServiceFramework
{
    public class IpcRequest
    {
        private Type[] _genericArguments = Array.Empty<Type>();

        public string MethodName { get; set; }
        public object[] Parameters { get; set; }

        /// <summary>
        /// Gets or sets the types of parameter of the IPC method to call
        /// </summary>
        public Type[] ParameterTypes { get; set; } = Array.Empty<Type>();

        public Type[] GenericArguments
        {
            get => _genericArguments ?? Array.Empty<Type>();
            set => _genericArguments = value;
        }
    }
}
