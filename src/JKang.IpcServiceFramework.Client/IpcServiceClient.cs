using Castle.DynamicProxy;
using JKang.IpcServiceFramework.IO;
using JKang.IpcServiceFramework.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework
{
    public abstract class IpcServiceClient<TInterface>
        where TInterface : class
    {
        protected IpcServiceClient()
            : this(new DefaultIpcMessageSerializer(), new DefaultValueConverter())
        { }

        protected IpcServiceClient(IIpcMessageSerializer serializer, IValueConverter converter)
        {
            this.Serializer = serializer;
            this.Converter = converter;
        }

        public IIpcMessageSerializer Serializer { get; }
        public IValueConverter Converter { get; }

        public async Task InvokeAsync(Expression<Action<TInterface>> exp)
        {
            IpcRequest request = GetRequest(exp, new MyInterceptor());
            IpcResponse response = await GetResponseAsync(request);

            if (response.Succeed)
            {
                return;
            }
            else
            {
                throw new InvalidOperationException(response.Failure);
            }
        }

        public async Task<TResult> InvokeAsync<TResult>(Expression<Func<TInterface, TResult>> exp)
        {
            IpcRequest request = GetRequest(exp, new MyInterceptor<TResult>());
            IpcResponse response = await GetResponseAsync(request);

            if (response.Succeed)
            {
                if (this.Converter.TryConvert(response.Data, typeof(TResult), out object @return))
                {
                    return (TResult)@return;
                }
                else
                {
                    throw new InvalidOperationException($"Unable to convert returned value to '{typeof(TResult).Name}'.");
                }
            }
            else
            {
                throw new InvalidOperationException(response.Failure);
            }
        }

        private static IpcRequest GetRequest(Expression exp, MyInterceptor interceptor)
        {
            if (!(exp is LambdaExpression lamdaExp))
            {
                throw new ArgumentException("Only support lamda expresion, ex: x => x.GetData(a, b)");
            }

            if (!(lamdaExp.Body is MethodCallExpression methodCallExp))
            {
                throw new ArgumentException("Only support calling method, ex: x => x.GetData(a, b)");
            }

            var proxyGenerator = new ProxyGenerator();
            TInterface proxy = proxyGenerator.CreateInterfaceProxyWithoutTarget<TInterface>(interceptor);
            Delegate @delegate = lamdaExp.Compile();
            @delegate.DynamicInvoke(proxy);

            return new IpcRequest
            {
                InterfaceName = typeof(TInterface).AssemblyQualifiedName,
                MethodName = interceptor.LastInvocation.Method.Name,
                Parameters = interceptor.LastInvocation.Arguments,
            };
        }

        protected abstract Task<IpcResponse> GetResponseAsync(IpcRequest request);

        protected IpcResponse GetIpcResponse(IpcRequest request, IpcReader reader, IpcWriter writer)
        {
            // send request
            writer.Write(request);

            // receive response
            return reader.ReadIpcResponse();
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
