using Castle.DynamicProxy;
using JKang.IpcServiceFramework.IO;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Client
{
    public abstract class IpcClient<TInterface> : IIpcClient<TInterface>
        where TInterface : class
    {
        private static readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();
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
            IpcRequest request = GetRequest(exp, new MyInterceptor());
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
            IpcRequest request = GetRequest(exp, new MyInterceptor<TResult>());
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
            IpcRequest request = GetRequest(exp, new MyInterceptor<Task>());
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
            IpcRequest request = GetRequest(exp, new MyInterceptor<Task<TResult>>());
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

        private static IpcRequest GetRequest(Expression exp, MyInterceptor interceptor)
        {
            if (!(exp is LambdaExpression lambdaExp))
            {
                throw new ArgumentException("Only support lambda expression, ex: x => x.GetData(a, b)");
            }

            if (!(lambdaExp.Body is MethodCallExpression methodCallExp))
            {
                throw new ArgumentException("Only support calling method, ex: x => x.GetData(a, b)");
            }

            TInterface proxy = _proxyGenerator.CreateInterfaceProxyWithoutTarget<TInterface>(interceptor);
            Delegate @delegate = lambdaExp.Compile();
            @delegate.DynamicInvoke(proxy);

            return new IpcRequest
            {
                MethodName = interceptor.LastInvocation.Method.Name,
                Parameters = interceptor.LastInvocation.Arguments,

                ParameterTypes = interceptor.LastInvocation.Method.GetParameters()
                              .Select(p => p.ParameterType)
                              .ToArray(),


                GenericArguments = interceptor.LastInvocation.GenericArguments,
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

        private class MyInterceptor : IInterceptor
        {
            public IInvocation LastInvocation { get; private set; }

            public virtual void Intercept(IInvocation invocation)
            {
                LastInvocation = invocation;
            }
        }

        private class MyInterceptor<TResult> : MyInterceptor
        {
            public override void Intercept(IInvocation invocation)
            {
                base.Intercept(invocation);
                invocation.ReturnValue = default(TResult);
            }
        }
    }
}
