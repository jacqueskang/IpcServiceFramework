using Microsoft.Extensions.DependencyInjection;
using System;

namespace JKang.IpcServiceFramework
{
    public interface IIpcServiceBuilder
    {
        IServiceCollection Services { get; }

        IIpcServiceBuilder AddService<TInterface, TImplementation>()
            where TInterface: class
            where TImplementation: class, TInterface;

        IIpcServiceBuilder AddService<TInterface, TImplementation>(Func<IServiceProvider, TImplementation> implementationFactory)
            where TInterface : class
            where TImplementation : class, TInterface;
    }
}