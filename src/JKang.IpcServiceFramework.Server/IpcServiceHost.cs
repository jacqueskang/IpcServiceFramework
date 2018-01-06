using JKang.IpcServiceFramework.IO;
using JKang.IpcServiceFramework.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO.Pipes;
using System.Reflection;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework
{
    public class IpcServiceHost : IIpcServiceHost
    {
        private readonly string _pipeName;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IpcServiceHost> _logger;
        private readonly IIpcMessageSerializer _serializer;
        private readonly IValueConverter _converter;

        public IpcServiceHost(string pipeName, IServiceProvider serviceProvider)
        {
            _pipeName = pipeName;
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetService<ILogger<IpcServiceHost>>();
            _serializer = _serviceProvider.GetRequiredService<IIpcMessageSerializer>();
            _converter = _serviceProvider.GetRequiredService<IValueConverter>();
        }

        public void Start()
        {
            using (var server = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1))
            using (var writer = new IpcWriter(server, _serializer))
            using (var reader = new IpcReader(server, _serializer))
            {
                _logger?.LogInformation("IPC server started.");
                while (true)
                {
                    _logger?.LogDebug("Waiting for connection...");
                    server.WaitForConnection();

                    try
                    {
                        _logger?.LogDebug("client connected, reading request...");
                        IpcRequest request = reader.ReadIpcRequest();

                        _logger?.LogDebug("request received, invoking corresponding method...");
                        IpcResponse response;
                        using (IServiceScope scope = _serviceProvider.CreateScope())
                        {
                            response = GetReponse(request, scope);
                        }

                        _logger?.LogDebug("sending response...");
                        writer.Write(response);

                        // disconnect client
                        server.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, ex.Message);
                        server.Disconnect();
                    }
                }
            }
        }

        private IpcResponse GetReponse(IpcRequest request, IServiceScope scope)
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
