using IpcServiceSample.ServiceContracts;
using JKang.IpcServiceFramework;
using System.Threading.Tasks;

namespace IpcServiceSample.ConsoleClient
{
    class MyClient : IpcServiceClient<IMyIpcService>, IMyIpcService
    {
        public MyClient(string pipeName)
            : base(pipeName)
        {
        }

        public Task<MyResponse> GetDataAsync(MyRequest request, bool iAmHandsome)
        {
            return InvokeAsync<MyResponse>(nameof(GetDataAsync), request, iAmHandsome);
        }
    }
}
