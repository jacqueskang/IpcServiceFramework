using JKang.IpcServiceFramework.IO;
using JKang.IpcServiceFramework.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO.Pipes;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.NamedPipe
{
    public class NamedPipeIpcServiceEndpoint : IpcServiceEndpoint
    {
        private readonly ILogger<NamedPipeIpcServiceEndpoint> _logger;
        private readonly NamedPipeOptions _options;
        private readonly IIpcMessageSerializer _serializer;
        private readonly IValueConverter _converter;

        public NamedPipeIpcServiceEndpoint(string name, IServiceProvider serviceProvider, string pipeName)
            : base(name, serviceProvider, NamedPipeConstants.ProtocolName)
        {
            PipeName = pipeName;

            _logger = serviceProvider.GetService<ILogger<NamedPipeIpcServiceEndpoint>>();
            _options = serviceProvider.GetRequiredService<NamedPipeOptions>();
            _serializer = serviceProvider.GetRequiredService<IIpcMessageSerializer>();
            _converter = serviceProvider.GetRequiredService<IValueConverter>();
        }

        public string PipeName { get; }


        public override void Listen()
        {
            NamedPipeOptions options = ServiceProvider.GetRequiredService<NamedPipeOptions>();

            var threads = new Thread[options.ThreadCount];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(StartServerThread);
                threads[i].Start();
            }

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

        private void StartServerThread(object obj)
        {
            using (var server = new NamedPipeServerStream(PipeName, PipeDirection.InOut, _options.ThreadCount))
            using (var writer = new IpcWriter(server, _serializer, leaveOpen: true))
            using (var reader = new IpcReader(server, _serializer, leaveOpen: true))
            {
                server.WaitForConnection();

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
                }
            }
        }

        private IpcResponse GetReponse(IpcRequest request, IServiceScope scope)
        {
            var @interface = Type.GetType(request.InterfaceName);
            if (@interface == null)
            {
                return IpcResponse.Fail($"Interface '{request.InterfaceName}' not found.");
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
