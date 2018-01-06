using IpcServiceSample.ServiceContracts;
using JKang.IpcServiceFramework;

namespace IpcServiceSample.ConsoleClient
{
    class ComputingServiceClient : IpcServiceClient<IComputingService>, IComputingService
    {
        public ComputingServiceClient(string pipeName)
            : base(pipeName)
        { }

        public ComplexNumber AddComplexNumber(ComplexNumber x, ComplexNumber y)
        {
            return Invoke<ComplexNumber>(nameof(AddComplexNumber), x, y);
        }

        public float AddFloat(float x, float y)
        {
            return Invoke<float>(nameof(AddFloat), x, y);
        }
    }
}
