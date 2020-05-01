using System;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Hosting
{
    public interface IIpcEndpoint: IDisposable
    {
        Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
