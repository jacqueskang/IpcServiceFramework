using JKang.IpcServiceFramework.IO;
using JKang.IpcServiceFramework.Services;
using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework
{
    public abstract class IpcServiceClient<TInterface>
    {
        private readonly string _pipeName;
        private readonly IIpcMessageSerializer _serializer;
        private readonly IValueConverter _converter;

        protected IpcServiceClient(string pipeName)
            : this(pipeName, new DefaultIpcMessageSerializer(), new DefaultValueConverter())
        { }

        protected IpcServiceClient(string pipeName,
            IIpcMessageSerializer serializer,
            IValueConverter converter)
        {
            _pipeName = pipeName;
            _serializer = serializer;
            _converter = converter;
        }

        protected TResult Invoke<TResult>(string method, params object[] args)
        {
            IpcRequest request = CreateRequest(method, args);
            IpcResponse response = GetResponseAsync(request).Result;

            if (response.Succeed)
            {
                if (_converter.TryConvert(response.Data, typeof(TResult), out object @return))
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

        protected void Invoke(string method, params object[] args)
        {
            IpcRequest request = CreateRequest(method, args);
            IpcResponse response = GetResponseAsync(request).Result;

            if (!response.Succeed)
            {
                throw new InvalidOperationException(response.Failure);
            }
        }

        private static IpcRequest CreateRequest(string method, object[] args)
        {
            return new IpcRequest
            {
                InterfaceName = typeof(TInterface).AssemblyQualifiedName,
                MethodName = method,
                Parameters = args,
            };
        }

        private async Task<IpcResponse> GetResponseAsync(IpcRequest request)
        {
            using (var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.None))
            using (var writer = new IpcWriter(client, _serializer))
            using (var reader = new IpcReader(client, _serializer))
            {
                await client.ConnectAsync();

                // send request
                writer.Write(request);

                // receive response
                return reader.ReadIpcResponse();
            }
        }
    }
}