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
            {
                await client.ConnectAsync();

                // send request
                var request = new IpcRequest
                {
                    InterfaceName = typeof(TInterface).AssemblyQualifiedName,
                    MethodName = method,
                    Parameters = args,
                };
                byte[] requestBin = _serializer.SerializeRequest(request);
                var writer = new BinaryWriter(client);
                writer.Write(requestBin.Length);
                writer.Write(requestBin);
                await client.FlushAsync();

                // receive response
                var reader = new BinaryReader(client);
                int responseLength = reader.ReadInt32();
                byte[] responseBin = reader.ReadBytes(responseLength);
                IpcResponse response = _serializer.DeserializeResponse(responseBin);
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