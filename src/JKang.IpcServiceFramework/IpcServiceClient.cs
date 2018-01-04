using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework
{
    public abstract class IpcServiceClient<TInterface>
    {
        private readonly string _pipeName;

        public IpcServiceClient(string pipeName)
        {
            _pipeName = pipeName;
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
                string json = JsonConvert.SerializeObject(request);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                var writer = new BinaryWriter(client);
                writer.Write(bytes.Length);
                writer.Write(bytes);
                await client.FlushAsync();

                // receive response
                var reader = new StreamReader(client);
                string responseJson = await reader.ReadToEndAsync();
                IpcResponse response = JsonConvert.DeserializeObject<IpcResponse>(responseJson);
                if (response.Succeed)
                {
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