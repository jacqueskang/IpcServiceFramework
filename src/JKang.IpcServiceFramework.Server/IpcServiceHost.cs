using JKang.IpcServiceFramework.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO.Pipes;
using System.Reflection;

namespace JKang.IpcServiceFramework
{
    public class IpcServiceHost : IIpcServiceHost
    {
        private readonly string _pipeName;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IpcServiceHost> _logger;
        private readonly IIpcMessageSerializer _serializer;

        public IpcServiceHost(string pipeName, IServiceProvider serviceProvider)
        {
            _pipeName = pipeName;
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetService<ILogger<IpcServiceHost>>();
            _serializer = _serviceProvider.GetRequiredService<IIpcMessageSerializer>();
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

        private static IpcResponse GetReponse(IpcRequest request, IServiceScope scope)
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
                if (request.Parameters[i].GetType() == paramInfos[i].ParameterType)
                {
                    args[i] = request.Parameters[i];
                }
                else if (request.Parameters[i] is JObject jObj)
                {
                    args[i] = jObj.ToObject(paramInfos[i].ParameterType);
                }
                else
                {
                    return IpcResponse.Fail($"Cannot convert value of parameter '{paramInfos[i].Name}'");
                }
            }

            try
            {
                dynamic @return = (dynamic)method.Invoke(service, args);
                return IpcResponse.Success(@return.Result); // TODO: handle non-waitable method
            }
            catch
            {
                return IpcResponse.Fail("Internal server error");
            }
        }
    }
}
