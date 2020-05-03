using JKang.IpcServiceFramework.Client;
using JKang.IpcServiceFramework.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using System;

namespace JKang.IpcServiceFramework.Testing
{
    public class IpcApplicationFactory<TContract> : IDisposable
        where TContract : class
    {
        private Func<IServiceProvider, TContract> _serviceFactory = _ => new Mock<TContract>().Object;
        private Action<IIpcHostBuilder> _ipcHostConfig = _ => { };
        private IHost _host = null;
        private bool _isDisposed = false;

        public IpcApplicationFactory<TContract> WithServiceImplementation(TContract serviceInstance)
        {
            _serviceFactory = _ => serviceInstance;
            return this;
        }

        public IpcApplicationFactory<TContract> WithServiceImplementation(Func<IServiceProvider, TContract> serviceFactory)
        {
            _serviceFactory = serviceFactory;
            return this;
        }

        public IpcApplicationFactory<TContract> WithIpcHostConfiguration(Action<IIpcHostBuilder> ipcHostConfig)
        {
            _ipcHostConfig = ipcHostConfig;
            return this;
        }

        public IIpcClient<TContract> CreateClient(Action<string, IServiceCollection> clientConfig)
        {
            if (clientConfig is null)
            {
                throw new ArgumentNullException(nameof(clientConfig));
            }

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(x => x.TryAddScoped(_serviceFactory))
                .ConfigureIpcHost(_ipcHostConfig)
                .Build();

            _host.StartAsync().Wait();

            string clientName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            clientConfig.Invoke(clientName, services);

            return services.BuildServiceProvider()
                .GetRequiredService<IIpcClientFactory<TContract>>()
                .CreateClient(clientName);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                _host?.Dispose();
            }

            _isDisposed = true;
        }
    }
}
