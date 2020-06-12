namespace JKang.IpcServiceFramework.Services
{
    public interface IIpcMessageSerializer
    {
        /// <exception cref="IpcSerializationException"></exception>
        byte[] SerializeRequest(IpcRequest request);

        /// <exception cref="IpcSerializationException"></exception>
        IpcResponse DeserializeResponse(byte[] binary);

        /// <exception cref="IpcSerializationException"></exception>
        IpcRequest DeserializeRequest(byte[] binary);

        /// <exception cref="IpcSerializationException"></exception>
        byte[] SerializeResponse(IpcResponse response);
    }
}
