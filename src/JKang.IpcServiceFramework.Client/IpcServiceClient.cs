using JKang.IpcServiceFramework.IO;
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

        public IpcServiceClient(string pipeName)
            : this(pipeName, new DefaultIpcMessageSerializer())
        { }

        public IpcServiceClient(string pipeName, IIpcMessageSerializer requestSerializer)
        {
            _pipeName = pipeName;
            _serializer = requestSerializer;
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
                    // TODO: handle primitive types
                    return ((JObject)response.Data).ToObject<TResult>();
                }
                else
                {
                    throw new InvalidOperationException(response.Failure);
                }
            }
        }
    }
}