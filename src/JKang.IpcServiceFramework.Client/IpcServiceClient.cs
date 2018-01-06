using JKang.IpcServiceFramework.IO;
using JKang.IpcServiceFramework.Services;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework
{
    public abstract class IpcServiceClient<TInterface>
    {
        private readonly string _pipeName;
        private readonly IIpcMessageSerializer _serializer;
        private readonly IValueConverter _converter;

        public IpcServiceClient(string pipeName)
            : this(pipeName, new DefaultIpcMessageSerializer(), new DefaultValueConverter())
        { }

        public IpcServiceClient(string pipeName,
            IIpcMessageSerializer serializer,
            IValueConverter converter)
        {
            _pipeName = pipeName;
            _serializer = serializer;
            _converter = converter;
        }

        public TResult Invoke<TResult>(string method, params object[] args)
        {
            return InvokeAsync<TResult>(method, args).Result;
        }

        public async Task<TResult> InvokeAsync<TResult>(string method, params object[] args)
        {
            using (var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.None))
            using (var writer = new IpcWriter(client, _serializer))
            using (var reader = new IpcReader(client, _serializer))
            {
                await client.ConnectAsync();

                // send request
                writer.Write(new IpcRequest
                {
                    InterfaceName = typeof(TInterface).AssemblyQualifiedName,
                    MethodName = method,
                    Parameters = args,
                });

                // receive response
                IpcResponse response = reader.ReadIpcResponse();
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
        }
    }
}