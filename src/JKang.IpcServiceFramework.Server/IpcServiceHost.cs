using JKang.IpcServiceFramework.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JKang.IpcServiceFramework.IO;

namespace JKang.IpcServiceFramework
{
    public abstract class IpcServiceHost : IIpcServiceHost
    {
        private readonly string _pipeName;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IpcServiceHost> _logger;
        private readonly IpcServiceOptions _options;
        private readonly IIpcMessageSerializer _serializer;
        private readonly IValueConverter _converter;

        protected IpcServiceHost(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetService<ILogger<IpcServiceHost>>();
            _options = _serviceProvider.GetRequiredService<IpcServiceOptions>();
            _serializer = _serviceProvider.GetRequiredService<IIpcMessageSerializer>();
            _converter = _serviceProvider.GetRequiredService<IValueConverter>();
        }

        protected IServiceProvider ServiceProvider => _serviceProvider;
        protected IpcServiceOptions Options => _options;
        protected IIpcMessageSerializer Serializer => _serializer;
        protected IValueConverter Converter => _converter;

        public virtual void Run()
        {
            Thread[] threads = new Thread[Options.ThreadCount];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(StartServerThread);
                threads[i].Start();
            }

            _logger?.LogInformation("Pipe IPC server started.");

            while (true)
            {
                Thread.Sleep(100);
                for (int i = 0; i < threads.Length; i++)
                {
                    if (threads[i].Join(250))
                    {
                        // thread is finished, starting a new thread
                        threads[i] = new Thread(StartServerThread);
                        threads[i].Start();
                    }
                }
            }
        }

        protected abstract void StartServerThread(object obj);

        protected void ProcessRequest(IpcReader reader, IpcWriter writer)
        {
            try
            {
                _logger?.LogDebug($"[thread {Thread.CurrentThread.ManagedThreadId}] client connected, reading request...");
                IpcRequest request = reader.ReadIpcRequest();

                _logger?.LogDebug($"[thread {Thread.CurrentThread.ManagedThreadId}] request received, invoking corresponding method...");
                IpcResponse response;
                using (IServiceScope scope = ServiceProvider.CreateScope())
                {
                    response = GetReponse(request, scope);
                }

                _logger?.LogDebug($"[thread {Thread.CurrentThread.ManagedThreadId}] sending response...");
                writer.Write(response);

                _logger?.LogDebug($"[thread {Thread.CurrentThread.ManagedThreadId}] done.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
                throw ex;
            }
        }

        protected IpcResponse GetReponse(IpcRequest request, IServiceScope scope)
        {
            var @interface = Type.GetType(request.InterfaceName);
            if (@interface == null)
            {
                return IpcResponse.Fail($"Interface '{@interface}' not found.");
            }

            object service = scope.ServiceProvider.GetService(@interface);
            if (service == null)
            {
                return IpcResponse.Fail($"No implementation of interface '{@interface}' found.");
            }

            MethodInfo method = service.GetType().GetMethod(request.MethodName);
            if (method == null)
            {
                return IpcResponse.Fail($"Method '{request.MethodName}' not found in interface '{@interface}'.");
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
