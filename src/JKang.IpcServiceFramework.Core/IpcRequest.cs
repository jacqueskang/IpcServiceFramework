// ---------------------------------------------------------------------------
// <copyright file="IpcRequest.cs" company="JKang">
// (c) All Rights Reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace JKang.IpcServiceFramework
{
    using System;

    /// <summary>
    /// The request packet for an IPC call
    /// </summary>
    public class IpcRequest
    {
        /// <summary>
        /// Contains the generic argument fields for the IPC method to call
        /// </summary>
        private Type[] genericArguments = new Type[0];

        /// <summary>
        /// Gets or sets the name of the IPC method to call
        /// </summary>
        public string MethodName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parameters of the IPC method to call
        /// </summary>
        public object[] Parameters { get; set; } = new object[0];

        /// <summary>
        /// Gets or sets the types of parameter of the IPC method to call
        /// </summary>
        public Type[] ParameterTypes { get; set; } = new Type[0];

        /// <summary>
        /// Gets or sets the generic arguments for the IPC method to call
        /// </summary>
        public Type[] GenericArguments
        {
            get => this.genericArguments ?? new Type[0];
            set => this.genericArguments = value;
        }
    }
}
