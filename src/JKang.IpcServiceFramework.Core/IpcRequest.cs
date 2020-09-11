using System;
using System.Collections.Generic;
using System.Reflection;
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
        public IEnumerable<IpcRequestParameterType> ParameterTypesByName { get; set; }

        [DataMember]
        public IEnumerable<Type> GenericArguments { get; set; }

        [DataMember]
        public IEnumerable<IpcRequestParameterType> GenericArgumentsByName { get; set; }
    }

    /// <summary>
    /// Used to pass ParameterTypes annd GenericArguments by "Name" instead of by an explicit Type object.
    /// This allows for ParameterTypes to resolve properly even if the assembly version isn't an exact match on the Client & Host.
    /// </summary>
    [DataContract]
    public class IpcRequestParameterType
    {
        [DataMember]
        public string ParameterType { get; private set; }

        [DataMember]
        public string AssemblyName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IpcRequestParameterType"/> class.
        /// </summary>
        public IpcRequestParameterType()
        {
            ParameterType = null;
            AssemblyName = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IpcRequestParameterType"/> class.
        /// </summary>
        /// <param name="paramType">The type of parameter.</param>
        /// <exception cref="ArgumentNullException">paramType</exception>
        public IpcRequestParameterType(Type paramType)
        {
            if (paramType == null)
            {
                throw new ArgumentNullException(nameof(paramType));
            }

            ParameterType = paramType.FullName;
            AssemblyName = paramType.Assembly.GetName().Name;
        }
    }
}
