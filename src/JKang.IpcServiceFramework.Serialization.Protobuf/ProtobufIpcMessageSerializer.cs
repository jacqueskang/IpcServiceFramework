using ProtoBuf;
using System;
using System.IO;

namespace JKang.IpcServiceFramework.Serialization.Protobuf
{
    public class ProtobufIpcMessageSerializer : IIpcMessageSerializer
    {
        public IpcRequest DeserializeRequest(byte[] binary)
        {
            ProtobufIpcRequest request = Deserialize<ProtobufIpcRequest>(binary);
            return new IpcRequest
            {
                GenericArguments = request.GenericArguments,
                MethodName = request.MethodName,
                Parameters = request.Parameters,
                ParameterTypes = request.ParameterTypes,
            };
        }

        public IpcResponse DeserializeResponse(byte[] binary)
        {
            ProtobufIpcResponse response = Deserialize<ProtobufIpcResponse>(binary);
            return new IpcResponse(response.Status, response.Data, response.ErrorMessage, response.InnerException);
        }

        public byte[] SerializeRequest(IpcRequest request)
            => Serialize(new ProtobufIpcRequest
            {
                GenericArguments = request.GenericArguments,
                MethodName = request.MethodName,
                Parameters = request.Parameters,
                ParameterTypes = request.ParameterTypes
            });

        public byte[] SerializeResponse(IpcResponse response)
            => Serialize(new ProtobufIpcResponse
            {
                Data = response.Data,
                ErrorMessage = response.ErrorMessage,
                InnerException = response.InnerException,
                Status = response.Status,
            });

        private byte[] Serialize<T>(T obj)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, obj);
                    ms.Flush();
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new IpcSerializationException("Failed to serialize IPC message", ex);
            }
        }

        private T Deserialize<T>(byte[] binary)
        {
            try
            {
                using (var ms = new MemoryStream(binary))
                {
                    return Serializer.Deserialize<T>(ms);
                }
            }
            catch (Exception ex)
            {
                throw new IpcSerializationException("Failed to deserialize IPC message", ex);
            }
        }
    }
}
