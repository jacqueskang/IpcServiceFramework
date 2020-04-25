using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Client
{
    public interface IIpcClient<TInterface>
        where TInterface : class
    {
        Task InvokeAsync(
            Expression<Action<TInterface>> exp,
            CancellationToken cancellationToken = default);

        Task<TResult> InvokeAsync<TResult>(
            Expression<Func<TInterface, TResult>> exp,
            CancellationToken cancellationToken = default);

        Task InvokeAsync(
            Expression<Func<TInterface, Task>> exp,
            CancellationToken cancellationToken = default);

        Task<TResult> InvokeAsync<TResult>(
            Expression<Func<TInterface, Task<TResult>>> exp,
            CancellationToken cancellationToken = default);
    }
}
