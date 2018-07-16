using System;
using JKang.IpcServiceFramework.Services;

namespace JKang.IpcServiceFramework
{
    public class IpcServiceClientBuilder<TInterface>
        where TInterface : class
    {
        private IIpcMessageSerializer _serializer = new DefaultIpcMessageSerializer();
        private IValueConverter _valueConverter = new DefaultValueConverter();
        private Func<IIpcMessageSerializer, IValueConverter, IpcServiceClient<TInterface>> _factory = null;

        public IpcServiceClientBuilder<TInterface> WithIpcMessageSerializer(IIpcMessageSerializer serializer)
        {
            _serializer = serializer;
            return this;
        }

        public IpcServiceClientBuilder<TInterface> WithValueConverter(IValueConverter valueConverter)
        {
            _valueConverter = valueConverter;
            return this;
        }

        public IpcServiceClientBuilder<TInterface> SetFactory(Func<IIpcMessageSerializer, IValueConverter, IpcServiceClient<TInterface>> factory)
        {
            _factory = factory;
            return this;
        }

        public IpcServiceClient<TInterface> Build()
        {
            if (_factory == null)
            {
                throw new InvalidOperationException("Client factory is not set.");
            }

            return _factory(_serializer, _valueConverter);
        }
    }
}
