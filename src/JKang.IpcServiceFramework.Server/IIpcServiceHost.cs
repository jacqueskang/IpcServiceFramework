using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework
{
    public interface IIpcServiceHost
    {
        void Run();

        Task RunAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}