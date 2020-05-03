using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JKang.IpcServiceFramework.Client
{
    internal class IpcClientFactory<TContract, TIpcClientOptions> : IIpcClientFactory<TContract>
        where TContract : class
        where TIpcClientOptions : IpcClientOptions
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IpcClientRegistration<TContract, TIpcClientOptions>> _registrations;

        public IpcClientFactory(
            IServiceProvider serviceProvider,
            IEnumerable<IpcClientRegistration<TContract, TIpcClientOptions>> registrations)
        {
            _serviceProvider = serviceProvider;
            _registrations = registrations;
        }

        public IIpcClient<TContract> CreateClient(string name)
        {
            IpcClientRegistration<TContract, TIpcClientOptions> registration = _registrations.FirstOrDefault(x => x.Name == name);
            if (registration == null)
            {
                throw new ArgumentException($"IPC client '{name}' is not configured.", nameof(name));
            }

            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                return registration.CreateClient(scope.ServiceProvider);
            }
        }
    }
}
