using JKang.IpcServiceFramework.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Hosting
{
    public abstract class IpcEndpoint<TContract> : IIpcEndpoint
        where TContract : class
    {
        private readonly IpcEndpointOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _semaphore;

        protected IpcEndpoint(
            IpcEndpointOptions options,
            IServiceProvider serviceProvider,
            ILogger logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _semaphore = new SemaphoreSlim(options.MaxConcurrentCalls);
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new TaskFactory(TaskScheduler.Default);
            while (!stoppingToken.IsCancellationRequested)
            {
                await _semaphore.WaitAsync(stoppingToken).ConfigureAwait(false);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                WaitAndProcessAsync(ProcessAsync, stoppingToken).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        _logger.LogError(task.Exception, "Error occurred");
                    }
                    return _semaphore.Release();
                }, TaskScheduler.Default);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        protected abstract Task WaitAndProcessAsync(Func<Stream, CancellationToken, Task> process, CancellationToken stoppingToken);

        private async Task ProcessAsync(Stream server, CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            if (_options.StreamTranslator != null)
            {
                server = _options.StreamTranslator(server);
            }

            using (var writer = new IpcWriter(server, _options.Serializer, leaveOpen: true))
            using (var reader = new IpcReader(server, _options.Serializer, leaveOpen: true))
            using (IDisposable loggingScope = _logger.BeginScope(new Dictionary<string, object>
            {
                { "threadId", Thread.CurrentThread.ManagedThreadId }
            }))
            {
                try
                {
                    IpcRequest request;
                    try
                    {
                        _logger.LogDebug($"Client connected, reading request...");
                        request = await reader.ReadIpcRequestAsync(stoppingToken).ConfigureAwait(false);
                    }
                    catch (IpcSerializationException ex)
                    {
                        throw new IpcFaultException(IpcStatus.BadRequest, "Failed to deserialize request.", ex);
                    }

                    stoppingToken.ThrowIfCancellationRequested();

                    IpcResponse response;
                    try
                    {
                        _logger.LogDebug($"Request received, invoking '{request.MethodName}'...");
                        using (IServiceScope scope = _serviceProvider.CreateScope())
                        {
                            response = await GetResponseAsync(request, scope).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex) when (!(ex is IpcException))
                    {
                        throw new IpcFaultException(IpcStatus.InternalServerError,
                            "Unexpected exception raised from user code", ex);
                    }

                    stoppingToken.ThrowIfCancellationRequested();

                    try
                    {
                        _logger.LogDebug($"Sending response...");
                        await writer.WriteAsync(response, stoppingToken).ConfigureAwait(false);
                    }
                    catch (IpcSerializationException ex)
                    {
                        throw new IpcFaultException(IpcStatus.InternalServerError,
                            "Failed to serialize response.", ex);
                    }

                    _logger.LogDebug($"Process finished.");
                }
                catch (IpcCommunicationException ex)
                {
                    _logger.LogError(ex, "Communication error occurred.");
                    // if communication error occurred, client will probably not receive any response 
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogWarning(ex, "IPC request process cancelled");
                    IpcResponse response = _options.IncludeFailureDetailsInResponse
                        ? IpcResponse.InternalServerError("IPC request process cancelled")
                        : IpcResponse.InternalServerError();
                    await writer.WriteAsync(response, stoppingToken).ConfigureAwait(false);
                }
                catch (IpcFaultException ex)
                {
                    _logger.LogError(ex, "Failed to process IPC request.");
                    IpcResponse response;
                    switch (ex.Status)
                    {
                        case IpcStatus.BadRequest:
                            response = _options.IncludeFailureDetailsInResponse
                                ? IpcResponse.BadRequest(ex.Message, ex.InnerException)
                                : IpcResponse.BadRequest();
                            break;
                        default:
                            response = _options.IncludeFailureDetailsInResponse
                                ? IpcResponse.InternalServerError(ex.Message, ex.InnerException)
                                : IpcResponse.InternalServerError();
                            break;
                    }
                    await writer.WriteAsync(response, stoppingToken).ConfigureAwait(false);
                }
            }
        }

        private async Task<IpcResponse> GetResponseAsync(IpcRequest request, IServiceScope scope)
        {
            object service = scope.ServiceProvider.GetService<TContract>();
            if (service == null)
            {
                throw new IpcFaultException(IpcStatus.BadRequest,
                    $"No implementation of interface '{typeof(TContract).FullName}' found.");
            }

            MethodInfo method = GetUnambiguousMethod(request, service);

            if (method == null)
            {
                throw new IpcFaultException(IpcStatus.BadRequest,
                    $"Method '{request.MethodName}' not found in interface '{typeof(TContract).FullName}'.");
            }

            ParameterInfo[] paramInfos = method.GetParameters();
            object[] requestParameters = request.Parameters?.ToArray() ?? Array.Empty<object>();
            if (paramInfos.Length != requestParameters.Length)
            {
                throw new IpcFaultException(IpcStatus.BadRequest,
                    $"Method '{request.MethodName}' expects {paramInfos.Length} parameters.");
            }

            Type[] genericArguments = method.GetGenericArguments();
            Type[] requestGenericArguments;

            if (request.GenericArguments != null)
            {
                // Generic arguments passed by Type
                requestGenericArguments = request.GenericArguments.ToArray();
            }
            else if (request.GenericArgumentsByName != null)
            {
                // Generic arguments passed by name
                requestGenericArguments = new Type[request.GenericArgumentsByName.Count()];
                int i = 0;
                foreach (var pair in request.GenericArgumentsByName)
                {
                    var a = Assembly.Load(pair.AssemblyName);
                    requestGenericArguments[i++] = a.GetType(pair.ParameterType);
                }
            }
            else
            {
                requestGenericArguments = Array.Empty<Type>();
            }

            if (genericArguments.Length != requestGenericArguments.Length)
            {
                throw new IpcFaultException(IpcStatus.BadRequest,
                    $"Generic arguments mismatch.");
            }

            object[] args = new object[paramInfos.Length];
            for (int i = 0; i < args.Length; i++)
            {
                object origValue = requestParameters[i];
                Type destType = paramInfos[i].ParameterType;
                if (destType.IsGenericParameter)
                {
                    destType = requestGenericArguments[destType.GenericParameterPosition];
                }

                if (_options.ValueConverter.TryConvert(origValue, destType, out object arg))
                {
                    args[i] = arg;
                }
                else
                {
                    throw new IpcFaultException(IpcStatus.BadRequest,
                        $"Cannot convert value of parameter '{paramInfos[i].Name}' ({origValue}) from {origValue.GetType().Name} to {destType.Name}.");
                }
            }

            if (method.IsGenericMethod)
            {
                method = method.MakeGenericMethod(requestGenericArguments);
            }

            object @return = method.Invoke(service, args);

            if (@return is Task task)
            {
                await task.ConfigureAwait(false);

                PropertyInfo resultProperty = @return.GetType().GetProperty("Result");
                return IpcResponse.Success(resultProperty?.GetValue(@return));
            }
            else
            {
                return IpcResponse.Success(@return);
            }
        }

        private static MethodInfo GetUnambiguousMethod(IpcRequest request, object service)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (service is null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            MethodInfo method = null;     // disambiguate - can't just call as before with generics - MethodInfo method = service.GetType().GetMethod(request.MethodName);

            Type[] types = service.GetType().GetInterfaces();

            IEnumerable<MethodInfo> allMethods = types.SelectMany(t => t.GetMethods());

            var serviceMethods = allMethods.Where(t => t.Name == request.MethodName).ToList();

            // Check if we were passed Type objects or IpcRequestParameterType objects
            if ((request.ParameterTypes != null) && (request.ParameterTypesByName != null))
            {
                throw new IpcFaultException(IpcStatus.BadRequest, "Only one of ParameterTypes and ParameterTypesByName should be set!");
            }
            if ((request.GenericArguments != null) && (request.GenericArgumentsByName != null))
            {
                throw new IpcFaultException(IpcStatus.BadRequest, "Only one of GenericArguments and GenericArgumentsByName should be set!");
            }

            object[] requestParameters = request.Parameters?.ToArray() ?? Array.Empty<object>();

            Type[] requestGenericArguments;
            if (request.GenericArguments != null)
            {
                // Generic arguments passed by Type
                requestGenericArguments = request.GenericArguments.ToArray();
            }
            else if (request.GenericArgumentsByName != null)
            {
                // Generic arguments passed by name
                requestGenericArguments = new Type[request.GenericArgumentsByName.Count()];
                int i = 0;
                foreach (var pair in request.GenericArgumentsByName)
                {
                    var a = Assembly.Load(pair.AssemblyName);
                    requestGenericArguments[i++] = a.GetType(pair.ParameterType);
                }
            }
            else
            {
                requestGenericArguments = Array.Empty<Type>();
            }

            Type[] requestParameterTypes;
            if (request.ParameterTypes != null)
            {
                // Parameter types passed by Type
                requestParameterTypes = request.ParameterTypes.ToArray();
            }
            else if (request.ParameterTypesByName != null)
            {
                // Parameter types passed by name
                requestParameterTypes = new Type[request.ParameterTypesByName.Count()];
                int i = 0;
                foreach (var pair in request.ParameterTypesByName)
                {
                    var a = Assembly.Load(pair.AssemblyName);
                    requestParameterTypes[i++] = a.GetType(pair.ParameterType);
                }
            }
            else
            {
                requestParameterTypes = Array.Empty<Type>();
            }

            foreach (MethodInfo serviceMethod in serviceMethods)
            {
                ParameterInfo[] serviceMethodParameters = serviceMethod.GetParameters();
                int parameterTypeMatches = 0;

                if (serviceMethodParameters.Length == requestParameters.Length && serviceMethod.GetGenericArguments().Length == requestGenericArguments.Length)
                {
                    for (int parameterIndex = 0; parameterIndex < serviceMethodParameters.Length; parameterIndex++)
                    {
                        Type serviceParameterType = serviceMethodParameters[parameterIndex].ParameterType.IsGenericParameter ?
                                            requestGenericArguments[serviceMethodParameters[parameterIndex].ParameterType.GenericParameterPosition] :
                                            serviceMethodParameters[parameterIndex].ParameterType;

                        if (serviceParameterType == requestParameterTypes[parameterIndex])
                        {
                            parameterTypeMatches++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (parameterTypeMatches == serviceMethodParameters.Length)
                    {
                        method = serviceMethod;        // signatures match so assign
                        break;
                    }
                }
            }

            return method;
        }

        #region IDisposable

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _semaphore.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
