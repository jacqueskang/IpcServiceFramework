using System;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Hosting
{
    public interface IIpcEndpoint: IDisposable
    {
        string Name { get; }

        Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
