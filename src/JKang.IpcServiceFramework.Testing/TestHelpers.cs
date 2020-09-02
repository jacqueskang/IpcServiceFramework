using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit.Sdk;

namespace JKang.IpcServiceFramework.Testing
{
    public static class TestHelpers
    {
        /// <summary>
        /// Creates an IPC request for the given method in the given interface.
        /// </summary>
        /// <param name="interfaceType">The Type of the interface containing the method.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="args">The arguments to the method.</param>
        /// <returns>IpcRequest object</returns>
        public static IpcRequest CreateIpcRequest(Type interfaceType, string methodName, params object[] args)
        {
            MethodInfo method = null;

            // Try to find the matching method based on name and args
            if (args.All(x => x != null))
            {
                method = interfaceType.GetMethod(methodName, args.Select(x => x.GetType()).ToArray());
            }

            if (method == null)
            {
                method = interfaceType.GetMethod(methodName);
            }

            if (method == null)
            {
                throw new ArgumentException($"Could not find a valid method in {interfaceType}!");
            }

            if (method.IsGenericMethod)
            {
                throw new ArgumentException($"{methodName} is generic and not supported!");
            }

            var methodParams = method.GetParameters();

            var request = new IpcRequest()
            {
                MethodName = methodName,
                Parameters = args
            };

            var parameterTypes = new Type[methodParams.Length];
            for (int i = 0; i < args.Length; i++)
            {
                parameterTypes[i] = methodParams[i].ParameterType; 
            }

            request.ParameterTypes = parameterTypes;

            return request;
        }


        /// <summary>
        /// Creates an IPC request for the given method name which takes no parameters.
        /// </summary>
        /// <param name="methodName">Name of the method. The method should have no parameters.</param>
        /// <returns>IpcRequest object</returns>
        public static IpcRequest CreateIpcRequest(string methodName)
        {
            return new IpcRequest()
            {
                MethodName = methodName,
                Parameters = Array.Empty<object>(),
                ParameterTypes = Array.Empty<Type>()
            };
        }
    }
}
