using JKang.IpcServiceFramework.IO;
using JKang.IpcServiceFramework.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework
{
    public abstract class IpcServiceEndpoint
    {
        protected IpcServiceEndpoint(string name, IServiceProvider serviceProvider)
        {
            Name = name;
            ServiceProvider = serviceProvider;
        }

        public string Name { get; }
        public IServiceProvider ServiceProvider { get; }

        public abstract void Listen();
    }

    public abstract class IpcServiceEndpoint<TContract>: IpcServiceEndpoint
        where TContract: class
    {
        private readonly IValueConverter _converter;
        private readonly IIpcMessageSerializer _serializer;

        protected IpcServiceEndpoint(string name, IServiceProvider serviceProvider)
            : base(name, serviceProvider)
        {
            _converter = serviceProvider.GetRequiredService<IValueConverter>();
            _serializer = serviceProvider.GetRequiredService<IIpcMessageSerializer>();
        }

        protected void Process(Stream server, ILogger logger)
        {
            using (var writer = new IpcWriter(server, _serializer, leaveOpen: true))
            using (var reader = new IpcReader(server, _serializer, leaveOpen: true))
            {
                try
                {
                    logger?.LogDebug($"[thread {Thread.CurrentThread.ManagedThreadId}] client connected, reading request...");
                    IpcRequest request = reader.ReadIpcRequest();

                    logger?.LogDebug($"[thread {Thread.CurrentThread.ManagedThreadId}] request received, invoking corresponding method...");
                    IpcResponse response;
                    using (IServiceScope scope = ServiceProvider.CreateScope())
                    {
                        response = GetReponse(request, scope);
                    }

                    logger?.LogDebug($"[thread {Thread.CurrentThread.ManagedThreadId}] sending response...");
                    writer.Write(response);

                    logger?.LogDebug($"[thread {Thread.CurrentThread.ManagedThreadId}] done.");
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, ex.Message);
                }
            }
        }

        protected IpcResponse GetReponse(IpcRequest request, IServiceScope scope)
        {
            object service = scope.ServiceProvider.GetService<TContract>();
            if (service == null)
            {
                return IpcResponse.Fail($"No implementation of interface '{typeof(TContract).FullName}' found.");
            }

            MethodInfo method = service.GetType().GetMethod(request.MethodName);
            if (method == null)
            {
                return IpcResponse.Fail($"Method '{request.MethodName}' not found in interface '{typeof(TContract).FullName}'.");
            }

            ParameterInfo[] paramInfos = method.GetParameters();
            if (paramInfos.Length != request.Parameters.Length)
            {
                return IpcResponse.Fail($"Parameter mismatch.");
            }

            object[] args = new object[paramInfos.Length];
            for (int i = 0; i < args.Length; i++)
            {
                object origValue = request.Parameters[i];
                Type destType = paramInfos[i].ParameterType;
                if (_converter.TryConvert(origValue, destType, out object arg))
                {
                    args[i] = arg;
                }
                else
                {
                    return IpcResponse.Fail($"Cannot convert value of parameter '{paramInfos[i].Name}' ({origValue}) from {origValue.GetType().Name} to {destType.Name}.");
                }
            }

            try
            {
                object @return = method.Invoke(service, args);
                if (@return is Task)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    return IpcResponse.Success(@return);
                }
            }
            catch (Exception ex)
            {
                return IpcResponse.Fail($"Internal server error: {ex.Message}");
            }
        }
    }
}