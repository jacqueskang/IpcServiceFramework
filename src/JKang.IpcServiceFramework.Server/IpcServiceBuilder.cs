using Microsoft.Extensions.DependencyInjection;

namespace JKang.IpcServiceFramework
{
    internal class IpcServiceBuilder : IIpcServiceBuilder
    {
        public IpcServiceBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        IIpcServiceBuilder IIpcServiceBuilder.AddService<TInterface, TImplementation>()
        {
            Services.AddScoped<TInterface, TImplementation>();
            return this;
        }
    }
}