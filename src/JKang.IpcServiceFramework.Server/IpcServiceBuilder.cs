using Microsoft.Extensions.DependencyInjection;
using System;

namespace JKang.IpcServiceFramework
{
    internal class IpcServiceBuilder : IIpcServiceBuilder
    {
        public IpcServiceBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public IIpcServiceBuilder AddService<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface
        {
            Services.AddScoped<TInterface, TImplementation>();
            return this;
        }

        public IIpcServiceBuilder AddService<TInterface, TImplementation>(Func<IServiceProvider, TImplementation> implementationFactory)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            Services.AddScoped<TInterface, TImplementation>(implementationFactory);
            return this;
        }
    }
}