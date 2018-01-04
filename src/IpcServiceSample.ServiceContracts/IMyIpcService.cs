using System.Threading.Tasks;

namespace IpcServiceSample
{
    public interface IMyIpcService
    {
        Task<MyResponse> GetDataAsync(MyRequest request, bool iAmHandsome);
    }
}
