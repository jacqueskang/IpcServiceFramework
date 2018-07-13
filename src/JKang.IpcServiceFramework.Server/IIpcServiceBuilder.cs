using Microsoft.Extensions.DependencyInjection;

namespace JKang.IpcServiceFramework
{
    public interface IIpcServiceBuilder
    {
        IServiceCollection Services { get; }

        IIpcServiceBuilder AddService<TInterface, TImplementation>()
            where TInterface: class
            where TImplementation: class, TInterface;
    }
}