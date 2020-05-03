using System;

namespace JKang.IpcServiceFramework.Client
{
    public class IpcClientRegistration<TContract, TIpcClientOptions>
        where TContract: class
        where TIpcClientOptions: IpcClientOptions
    {
        private readonly Func<IServiceProvider, TIpcClientOptions, IIpcClient<TContract>> _clientFactory;
        private readonly Action<IServiceProvider, TIpcClientOptions> _configureOptions;

        public IpcClientRegistration(string name,
            Func<IServiceProvider, TIpcClientOptions, IIpcClient<TContract>> clientFactory,
            Action<IServiceProvider, TIpcClientOptions> configureOptions)
        {
            Name = name;
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _configureOptions = configureOptions;
        }

        public string Name { get; }

        public IIpcClient<TContract> CreateClient(IServiceProvider serviceProvider)
        {
            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            TIpcClientOptions options = Activator.CreateInstance<TIpcClientOptions>();
            _configureOptions?.Invoke(serviceProvider, options);
            return _clientFactory.Invoke(serviceProvider, options);
        }
    }
}
