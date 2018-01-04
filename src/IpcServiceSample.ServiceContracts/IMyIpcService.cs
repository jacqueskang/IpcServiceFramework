using System.Threading.Tasks;

namespace IpcServiceSample.ServiceContracts
{
    public interface IMyIpcService
    {
        Task<MyResponse> GetDataAsync(MyRequest request, bool iAmHandsome);
    }
}
