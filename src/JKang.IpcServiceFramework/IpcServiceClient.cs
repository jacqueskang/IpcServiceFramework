namespace JKang.IpcServiceFramework
{
    public class IpcServiceClient<TInterface>
    {
        private readonly string _pipeName;

        public IpcServiceClient(string pipeName)
        {
            _pipeName = pipeName;
        }
    }
}