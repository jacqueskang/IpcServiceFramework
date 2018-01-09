using Microsoft.Extensions.DependencyInjection;

namespace JKang.IpcServiceFramework
{
    internal class IpcServiceCollection : IIpcServiceCollection
    {
        private readonly IServiceCollection _services;

        public IpcServiceCollection(IServiceCollection services)
        {
            _services = services;
        }

        IIpcServiceCollection IIpcServiceCollection.AddService<TInterface, TImplementation>()
        {
            _services.AddScoped<TInterface, TImplementation>();
            return this;
        }
    }
}