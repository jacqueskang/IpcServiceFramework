using JKang.IpcServiceFramework.IO;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Client
{
    public abstract class IpcClient<TInterface> : IIpcClient<TInterface>
        where TInterface : class
    {
        private readonly IpcClientOptions _options;

        protected IpcClient(
            string name,
            IpcClientOptions options)
        {
            Name = name;
            _options = options;
        }

        public string Name { get; }

#if !DISABLE_DYNAMIC_CODE_GENERATION
        /// <exception cref="IpcSerializationException">If unable to serialize request</exception>
        /// <exception cref="IpcCommunicationException">If communication is broken</exception>
        /// <exception cref="IpcFaultException">If error occurred in server</exception>
        public async Task InvokeAsync(Expression<Action<TInterface>> exp,
            CancellationToken cancellationToken = default)
        {
            var request = GetRequest(exp, DispatchProxy.Create<TInterface, IpcProxy>());
            var response = await GetResponseAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.Succeed())
            {
                throw response.CreateFaultException();
            }
        }

        /// <exception cref="IpcSerializationException">If unable to serialize request</exception>
        /// <exception cref="IpcCommunicationException">If communication is broken</exception>
        /// <exception cref="IpcFaultException">If error occurred in server</exception>
        public async Task<TResult> InvokeAsync<TResult>(Expression<Func<TInterface, TResult>> exp,
            CancellationToken cancellationToken = default)
        {
            var request = GetRequest(exp, DispatchProxy.Create<TInterface, IpcProxy<TResult>>());
            var response = await GetResponseAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.Succeed())
            {
                throw response.CreateFaultException();
            }

            if (!_options.ValueConverter.TryConvert(response.Data, typeof(TResult), out object @return))
            {
                throw new IpcSerializationException($"Unable to convert returned value to '{typeof(TResult).Name}'.");
            }

            return (TResult)@return;
        }

        /// <exception cref="IpcSerializationException">If unable to serialize request</exception>
        /// <exception cref="IpcCommunicationException">If communication is broken</exception>
        /// <exception cref="IpcFaultException">If error occurred in server</exception>
        public async Task InvokeAsync(Expression<Func<TInterface, Task>> exp,
            CancellationToken cancellationToken = default)
        {
            var request = GetRequest(exp, DispatchProxy.Create<TInterface, IpcProxy<Task>>());
            var response = await GetResponseAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.Succeed())
            {
                throw response.CreateFaultException();
            }
        }

        /// <exception cref="IpcSerializationException">If unable to serialize request</exception>
        /// <exception cref="IpcCommunicationException">If communication is broken</exception>
        /// <exception cref="IpcFaultException">If error occurred in server</exception>
        public async Task<TResult> InvokeAsync<TResult>(Expression<Func<TInterface, Task<TResult>>> exp,
            CancellationToken cancellationToken = default)
        {
            var request = GetRequest(exp, DispatchProxy.Create<TInterface, IpcProxy<Task<TResult>>>());
            var response = await GetResponseAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.Succeed())
            {
                throw response.CreateFaultException();
            }

            if (_options.ValueConverter.TryConvert(response.Data, typeof(TResult), out object @return))
            {
                return (TResult)@return;
            }
            else
            {
                throw new IpcSerializationException($"Unable to convert returned value to '{typeof(TResult).Name}'.");
            }
        }
#endif

        public async Task<TResult> InvokeAsync<TResult>(IpcRequest request, CancellationToken cancellationToken = default)
        {
            var response = await GetResponseAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.Succeed())
            {
                throw response.CreateFaultException();
            }

            if (_options.ValueConverter.TryConvert(response.Data, typeof(TResult), out object @return))
            {
                return (TResult)@return;
            }
            else
            {
                throw new IpcSerializationException($"Unable to convert returned value to '{typeof(TResult).Name}'.");
            }
        }

        public async Task InvokeAsync(IpcRequest request, CancellationToken cancellationToken = default)
        {
            var response = await GetResponseAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.Succeed())
            {
                throw response.CreateFaultException();
            }
        }

#if !DISABLE_DYNAMIC_CODE_GENERATION
        private IpcRequest GetRequest(Expression exp, TInterface proxy)
        {
            if (!(exp is LambdaExpression lambdaExp))
            {
                throw new ArgumentException("Only support lambda expression, ex: x => x.GetData(a, b)");
            }

            if (!(lambdaExp.Body is MethodCallExpression methodCallExp))
            {
                throw new ArgumentException("Only support calling method, ex: x => x.GetData(a, b)");
            }

            Delegate @delegate = lambdaExp.Compile();
            @delegate.DynamicInvoke(proxy);

            if (_options.UseSimpleTypeNameAssemblyFormatHandling)
            {
                IpcRequestParameterType[] paramByName = null;
                IpcRequestParameterType[] genericByName = null;

                var parameterTypes = (proxy as IpcProxy).LastInvocation.Method.GetParameters().Select(p => p.ParameterType);

                if (parameterTypes.Any())
                {
                    paramByName = new IpcRequestParameterType[parameterTypes.Count()];
                    int i = 0;
                    foreach (var type in parameterTypes)
                    {
                        paramByName[i++] = new IpcRequestParameterType(type);
                    }
                }

                var genericTypes = (proxy as IpcProxy).LastInvocation.Method.GetGenericArguments();

                if (genericTypes.Length > 0)
                {
                    genericByName = new IpcRequestParameterType[genericTypes.Count()];
                    int i = 0;
                    foreach (var type in genericTypes)
                    {
                        genericByName[i++] = new IpcRequestParameterType(type);
                    }
                }


                return new IpcRequest
                {
                    MethodName = (proxy as IpcProxy).LastInvocation.Method.Name,
                    Parameters = (proxy as IpcProxy).LastInvocation.Arguments,

                    ParameterTypesByName = paramByName,
                    GenericArgumentsByName = genericByName
                };
            }
            else
            {
                return new IpcRequest
                {
                    MethodName = (proxy as IpcProxy).LastInvocation.Method.Name,
                    Parameters = (proxy as IpcProxy).LastInvocation.Arguments,

                    ParameterTypes = (proxy as IpcProxy).LastInvocation.Method.GetParameters()
                                  .Select(p => p.ParameterType)
                                  .ToArray(),


                    GenericArguments = (proxy as IpcProxy).LastInvocation.Method.GetGenericArguments(),
                };
            }
        }
#endif

        protected abstract Task<IpcStreamWrapper> ConnectToServerAsync(CancellationToken cancellationToken);

        private async Task<IpcResponse> GetResponseAsync(IpcRequest request, CancellationToken cancellationToken)
        {
            using (IpcStreamWrapper client = await ConnectToServerAsync(cancellationToken).ConfigureAwait(false))
            using (Stream client2 = _options.StreamTranslator == null ? client.Stream : _options.StreamTranslator(client.Stream))
            using (var writer = new IpcWriter(client2, _options.Serializer, leaveOpen: true))
            using (var reader = new IpcReader(client2, _options.Serializer, leaveOpen: true))
            {
                // send request
                await writer.WriteAsync(request, cancellationToken).ConfigureAwait(false);

                // receive response
                return await reader.ReadIpcResponseAsync(cancellationToken).ConfigureAwait(false);
            }
        }

#if !DISABLE_DYNAMIC_CODE_GENERATION
        public class IpcProxy : DispatchProxy
        {
            public Invocation LastInvocation { get; protected set; }

            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                LastInvocation = new Invocation(targetMethod, args);
                return null;
            }

            public class Invocation
            {
                public Invocation(MethodInfo method, object[] args)
                {
                    Method = method;
                    Arguments = args;
                }

                public MethodInfo Method { get; }
                public object[] Arguments { get; }
            }
        }

        public class IpcProxy<TResult> : IpcProxy
        {
            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                LastInvocation = new Invocation(targetMethod, args);
                return default(TResult);
            }
        }
#endif
    }
}
