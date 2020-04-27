using JKang.IpcServiceFramework.Hosting.Abstractions;
using JKang.IpcServiceFramework.IO;
using JKang.IpcServiceFramework.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
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
            string name,
            IpcEndpointOptions options,
            IServiceProvider serviceProvider,
            IIpcMessageSerializer serializer,
            IValueConverter valueConverter,
            ILogger logger)
        {
            Name = name;
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _valueConverter = valueConverter ?? throw new ArgumentNullException(nameof(valueConverter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _semaphore = new SemaphoreSlim(options.MaxConcurrentCalls);
        }

        public string Name { get; }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new TaskFactory(TaskScheduler.Default);
            while (!stoppingToken.IsCancellationRequested)
            {
                await _semaphore.WaitAsync(stoppingToken).ConfigureAwait(false);

                WaitAndProcessAsync(ProcessAsync, stoppingToken).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        _logger.LogError(task.Exception, "Error occurred");
                    }
                    return _semaphore.Release();
                });
            }
        }

        protected abstract Task WaitAndProcessAsync(Func<Stream, CancellationToken, Task> process, CancellationToken stoppingToken);

        protected virtual Stream TransformStream(Stream input) => input;

        private async Task ProcessAsync(Stream server, CancellationToken stoppingToken)
        {
            server = TransformStream(server);
            using (var writer = new IpcWriter(server, _serializer, leaveOpen: true))
            using (var reader = new IpcReader(server, _serializer, leaveOpen: true))
            {
                try
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }

                    _logger.LogDebug($"[thread {Thread.CurrentThread.ManagedThreadId}] client connected, reading request...");
                    IpcRequest request = await reader.ReadIpcRequestAsync(stoppingToken).ConfigureAwait(false);

                    stoppingToken.ThrowIfCancellationRequested();

                    _logger.LogDebug($"[thread {Thread.CurrentThread.ManagedThreadId}] request received, invoking '{request.MethodName}'...");
                    IpcResponse response;
                    using (IServiceScope scope = _serviceProvider.CreateScope())
                    {
                        response = await GetReponse(request, scope).ConfigureAwait(false);
                    }

                    stoppingToken.ThrowIfCancellationRequested();

                    _logger.LogDebug($"[thread {Thread.CurrentThread.ManagedThreadId}] sending response...");
                    await writer.WriteAsync(response, stoppingToken).ConfigureAwait(false);

                    _logger.LogDebug($"[thread {Thread.CurrentThread.ManagedThreadId}] done.");
                }
                catch (Exception ex) when (!(ex is IpcServerException))
                {
                    var response = IpcResponse.Fail(ex, _options.IncludeFailureDetailsInResponse);
                    _logger.LogError(ex, response.Failure);
                    await writer.WriteAsync(response, stoppingToken).ConfigureAwait(false);
                }
            }
        }

        private async Task<IpcResponse> GetReponse(IpcRequest request, IServiceScope scope)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            object service = scope.ServiceProvider.GetService<TContract>();
            if (service == null)
            {
                return IpcResponse.Fail($"No implementation of interface '{typeof(TContract).FullName}' found.");
            }

            MethodInfo method = GetUnambiguousMethod(request, service);

            if (method == null)
            {
                return IpcResponse.Fail($"Method '{request.MethodName}' not found in interface '{typeof(TContract).FullName}'.");
            }

            ParameterInfo[] paramInfos = method.GetParameters();
            if (paramInfos.Length != request.Parameters.Length)
            {
                return IpcResponse.Fail($"Parameter mismatch.");
            }

            Type[] genericArguments = method.GetGenericArguments();
            if (genericArguments.Length != request.GenericArguments.Length)
            {
                return IpcResponse.Fail($"Generic arguments mismatch.");
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
                    return IpcResponse.Fail($"Cannot convert value of parameter '{paramInfos[i].Name}' ({origValue}) from {origValue.GetType().Name} to {destType.Name}.");
                }
            }

            try
            {
                if (method.IsGenericMethod)
                {
                    method = method.MakeGenericMethod(request.GenericArguments);
                }

                object @return;
                try
                {
                    @return = method.Invoke(service, args);
                }
                catch (Exception ex)
                {
                    return IpcResponse.Fail(ex, _options.IncludeFailureDetailsInResponse, true);
                }

                if (@return is Task)
                {
                    await ((Task)@return).ConfigureAwait(false);

                    PropertyInfo resultProperty = @return.GetType().GetProperty("Result");
                    return IpcResponse.Success(resultProperty?.GetValue(@return));
                }
                else
                {
                    return IpcResponse.Success(@return);
                }
            }
            catch (Exception ex) when (!(ex is IpcServerException))
            {
                var response = IpcResponse.Fail(ex, _options.IncludeFailureDetailsInResponse);
                _logger.LogError(ex, response.Failure);
                return response;
            }
        }

        /// <summary>
        /// Get the method that matches the requested signature
        /// </summary>
        /// <param name="request">The service call request</param>
        /// <param name="service">The service</param>
        /// <returns>The disambiguated service method</returns>
        public static MethodInfo GetUnambiguousMethod(IpcRequest request, object service)
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
