namespace JKang.IpcServiceFramework
{
    public interface IIpcServiceCollection
    {
        IIpcServiceCollection AddService<TInterface, TImplementation>()
            where TInterface: class
            where TImplementation: class, TInterface;
    }
}