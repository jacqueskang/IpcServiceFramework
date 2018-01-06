using IpcServiceSample.ServiceContracts;
using JKang.IpcServiceFramework;

namespace IpcServiceSample.ConsoleClient
{
    class ComputingServiceClient : IpcServiceClient<IComputingService>, IComputingService
    {
        public ComputingServiceClient(string pipeName)
            : base(pipeName)
        { }

        public float Add(float x, float y)
        {
            return Invoke<float>(nameof(Add), x, y);
        }
    }
}
