using Newtonsoft.Json;
using System.Text;

namespace JKang.IpcServiceFramework.Services
{
    public class DefaultIpcMessageSerializer : IIpcMessageSerializer
    {
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        };

        public IpcRequest DeserializeRequest(byte[] binary)
        {
            return Deserialize<IpcRequest>(binary);
        }

        public IpcResponse DeserializeResponse(byte[] binary)
        {
            return Deserialize<IpcResponse>(binary);
        }

        public byte[] SerializeRequest(IpcRequest request)
        {
            return Serialize(request);
        }

        public byte[] SerializeResponse(IpcResponse response)
        {
            return Serialize(response);
        }

        private T Deserialize<T>(byte[] binary)
        {
            string json = Encoding.UTF8.GetString(binary);
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }

        private byte[] Serialize(object obj)
        {
            string json = JsonConvert.SerializeObject(obj, _settings);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
