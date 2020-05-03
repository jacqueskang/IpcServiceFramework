namespace JKang.IpcServiceFramework.Client
{
    public interface IIpcClientFactory<TContract>
        where TContract: class
    {
        IIpcClient<TContract> CreateClient(string name);
    }
}
