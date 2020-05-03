using JKang.IpcServiceFramework.Hosting.Abstractions;
using JKang.IpcServiceFramework.IO;
using JKang.IpcServiceFramework.Services;
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
        private readonly IIpcMessageSerializer _serializer;
        private readonly IValueConverter _valueConverter;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _semaphore;

        protected IpcEndpoint(
            IpcEndpointOptions options,
            IServiceProvider serviceProvider,
            IIpcMessageSerializer serializer,
            IValueConverter valueConverter,
            ILogger logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _valueConverter = valueConverter ?? throw new ArgumentNullException(nameof(valueConverter));
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

            using (var writer = new IpcWriter(server, _serializer, leaveOpen: true))
            using (var reader = new IpcReader(server, _serializer, leaveOpen: true))
            using (IDisposable loggingScope = _logger.BeginScope(new Dictionary<string, object>
            {
                { "threadId", Thread.CurrentThread.ManagedThreadId }
            }))
            {
                try
                {
                    _logger.LogDebug($"Client connected, reading request...");
                    IpcRequest request = await reader.ReadIpcRequestAsync(stoppingToken).ConfigureAwait(false);

                    stoppingToken.ThrowIfCancellationRequested();
                    _logger.LogDebug($"Request received, invoking '{request.MethodName}'...");
                    IpcResponse response;
                    using (IServiceScope scope = _serviceProvider.CreateScope())
                    {
                        response = await GetReponseAsync(request, scope).ConfigureAwait(false);
                    }

                    stoppingToken.ThrowIfCancellationRequested();
                    _logger.LogDebug($"Sending response...");
                    await writer.WriteAsync(response, stoppingToken).ConfigureAwait(false);

                    _logger.LogDebug($"Process finished.");
                }
                catch (IpcCommunicationException ex)
                {
                    _logger.LogError(ex, "Communication error occurred.");
                }
                catch (IpcSerializationException ex)
                {
                    _logger.LogError(ex, "Serialization error occurred.");
                }
                catch (IpcFaultException ex) // thrown by user
                {
                    _logger.LogWarning(ex, "Exception raised from user code");
                    IpcResponse response = _options.IncludeFailureDetailsInResponse
                        ? IpcResponse.InternalServerError("Exception raised from user code", ex)
                        : IpcResponse.InternalServerError();
                    await writer.WriteAsync(response, stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (!(ex is IpcException))
                {
                    _logger.LogError(ex, "Unexpected exception raised from user code");
                    IpcResponse response = _options.IncludeFailureDetailsInResponse
                        ? IpcResponse.InternalServerError("Unexpected error occurred", ex)
                        : IpcResponse.InternalServerError();
                    await writer.WriteAsync(response, stoppingToken).ConfigureAwait(false);
                }
            }
        }

        private async Task<IpcResponse> GetReponseAsync(IpcRequest request, IServiceScope scope)
        {
            object service = scope.ServiceProvider.GetService<TContract>();
            if (service == null)
            {
                return _options.IncludeFailureDetailsInResponse
                    ? IpcResponse.BadRequest($"No implementation of interface '{typeof(TContract).FullName}' found.")
                    : IpcResponse.BadRequest();
            }

            MethodInfo method = GetUnambiguousMethod(request, service);

            if (method == null)
            {
                return _options.IncludeFailureDetailsInResponse
                    ? IpcResponse.BadRequest($"Method '{request.MethodName}' not found in interface '{typeof(TContract).FullName}'.")
                    : IpcResponse.BadRequest();
            }

            ParameterInfo[] paramInfos = method.GetParameters();
            if (paramInfos.Length != request.Parameters.Length)
            {
                return _options.IncludeFailureDetailsInResponse
                    ? IpcResponse.BadRequest("Parameter mismatch.")
                    : IpcResponse.BadRequest();
            }

            Type[] genericArguments = method.GetGenericArguments();
            if (genericArguments.Length != request.GenericArguments.Length)
            {
                return _options.IncludeFailureDetailsInResponse
                    ? IpcResponse.BadRequest($"Generic arguments mismatch.")
                    : IpcResponse.BadRequest();
            }

            object[] args = new object[paramInfos.Length];
            for (int i = 0; i < args.Length; i++)
            {
                object origValue = request.Parameters[i];
                Type destType = paramInfos[i].ParameterType;
                if (destType.IsGenericParameter)
                {
                    destType = request.GenericArguments[destType.GenericParameterPosition];
                }

                if (_valueConverter.TryConvert(origValue, destType, out object arg))
                {
                    args[i] = arg;
                }
                else
                {
                    return _options.IncludeFailureDetailsInResponse
                        ? IpcResponse.BadRequest($"Cannot convert value of parameter '{paramInfos[i].Name}' ({origValue}) from {origValue.GetType().Name} to {destType.Name}.")
                        : IpcResponse.BadRequest();
                }
            }

            if (method.IsGenericMethod)
            {
                method = method.MakeGenericMethod(request.GenericArguments);
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
            if (request == null || service == null)
            {
                return null;
            }

            MethodInfo method = null;     // disambiguate - can't just call as before with generics - MethodInfo method = service.GetType().GetMethod(request.MethodName);

            Type[] types = service.GetType().GetInterfaces();

            System.Collections.Generic.IEnumerable<MethodInfo> allMethods = types.SelectMany(t => t.GetMethods());

            var serviceMethods = allMethods.Where(t => t.Name == request.MethodName).ToList();

            foreach (MethodInfo serviceMethod in serviceMethods)
            {
                ParameterInfo[] serviceMethodParameters = serviceMethod.GetParameters();
                int parameterTypeMatches = 0;

                if (serviceMethodParameters.Length == request.Parameters.Length && serviceMethod.GetGenericArguments().Length == request.GenericArguments.Length)
                {
                    for (int parameterIndex = 0; parameterIndex < serviceMethodParameters.Length; parameterIndex++)
                    {
                        Type serviceParameterType = serviceMethodParameters[parameterIndex].ParameterType.IsGenericParameter ?
                                            request.GenericArguments[serviceMethodParameters[parameterIndex].ParameterType.GenericParameterPosition] :
                                            serviceMethodParameters[parameterIndex].ParameterType;

                        if (serviceParameterType == request.ParameterTypes[parameterIndex])
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
