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

        /// <exception cref="IpcSerializationException">If unable to serialize request</exception>
        /// <exception cref="IpcCommunicationException">If communication is broken</exception>
        /// <exception cref="IpcFaultException">If error occurred in server</exception>
        public async Task InvokeAsync(Expression<Action<TInterface>> exp,
            CancellationToken cancellationToken = default)
        {
            IpcRequest request = GetRequest(exp, DispatchProxy.Create<TInterface, IpcProxy>());
            IpcResponse response = await GetResponseAsync(request, cancellationToken).ConfigureAwait(false);

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
            IpcRequest request = GetRequest(exp, DispatchProxy.Create<TInterface, IpcProxy<TResult>>());
            IpcResponse response = await GetResponseAsync(request, cancellationToken).ConfigureAwait(false);

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
            IpcRequest request = GetRequest(exp, DispatchProxy.Create<TInterface, IpcProxy<Task>>());
            IpcResponse response = await GetResponseAsync(request, cancellationToken).ConfigureAwait(false);

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
            IpcRequest request = GetRequest(exp, DispatchProxy.Create<TInterface, IpcProxy<Task<TResult>>>());
            IpcResponse response = await GetResponseAsync(request, cancellationToken).ConfigureAwait(false);

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

        private static IpcRequest GetRequest(Expression exp, TInterface proxy)
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

        protected abstract Task<Stream> ConnectToServerAsync(CancellationToken cancellationToken);

        private async Task<IpcResponse> GetResponseAsync(IpcRequest request, CancellationToken cancellationToken)
        {
            using (Stream client = await ConnectToServerAsync(cancellationToken).ConfigureAwait(false))
            using (Stream client2 = _options.StreamTranslator == null ? client : _options.StreamTranslator(client))
            using (var writer = new IpcWriter(client2, _options.Serializer, leaveOpen: true))
            using (var reader = new IpcReader(client2, _options.Serializer, leaveOpen: true))
            {
                // send request
                await writer.WriteAsync(request, cancellationToken).ConfigureAwait(false);

                // receive response
                return await reader.ReadIpcResponseAsync(cancellationToken).ConfigureAwait(false);
            }
        }

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
    }
}
