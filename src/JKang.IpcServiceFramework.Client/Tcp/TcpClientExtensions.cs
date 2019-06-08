using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JKang.IpcServiceFramework.Tcp
{
    internal static class TcpClientExtensions
    {
        public static Task ConnectAsync(this TcpClient client, string host, int port) => ConnectAsyncImpl(client, () => client.BeginConnect(host, port, null, null), CancellationToken.None);
        public static Task ConnectAsync(this TcpClient client, IPAddress address, int port) => ConnectAsyncImpl(client, () => client.BeginConnect(address, port, null, null), CancellationToken.None);
        public static Task ConnectAsync(this TcpClient client, IPEndPoint remoteEP) => ConnectAsyncImpl(client, () => client.BeginConnect(remoteEP.Address, remoteEP.Port, null, null), CancellationToken.None);
        public static Task ConnectAsync(this TcpClient client, IPAddress[] addresses, int port) => ConnectAsyncImpl(client, () => client.BeginConnect(addresses, port, null, null), CancellationToken.None);

        public static Task ConnectAsync(this TcpClient client, string host, int port, CancellationToken cancellationToken) => ConnectAsyncImpl(client, () => client.BeginConnect(host, port, null, null), cancellationToken);
        public static Task ConnectAsync(this TcpClient client, IPAddress address, int port, CancellationToken cancellationToken) => ConnectAsyncImpl(client, () => client.BeginConnect(address, port, null, null), cancellationToken);
        public static Task ConnectAsync(this TcpClient client, IPEndPoint remoteEP, CancellationToken cancellationToken) => ConnectAsyncImpl(client, () => client.BeginConnect(remoteEP.Address, remoteEP.Port, null, null), cancellationToken);
        public static Task ConnectAsync(this TcpClient client, IPAddress[] addresses, int port, CancellationToken cancellationToken) => ConnectAsyncImpl(client, () => client.BeginConnect(addresses, port, null, null), cancellationToken);

        static async Task ConnectAsyncImpl(TcpClient client, Func<IAsyncResult> beginConnect, CancellationToken cancellationToken)
        {
            var asyncResult = beginConnect();

            CancellationTokenRegistration tokenRegistration = default(CancellationTokenRegistration);
            RegisteredWaitHandle waitHandleRegistration = null;

            var completionSource = new TaskCompletionSource<bool>();

            try
            {
                waitHandleRegistration = ThreadPool.RegisterWaitForSingleObject(
                    asyncResult.AsyncWaitHandle,
                    (state, timeout) => completionSource.SetResult(!timeout),
                    null,
                    Timeout.Infinite,
                    executeOnlyOnce: true);

                tokenRegistration = cancellationToken.Register(() => client.Close());

                await completionSource.Task;
            }
            finally
            {
                waitHandleRegistration?.Unregister(asyncResult.AsyncWaitHandle);
                tokenRegistration.Dispose();
            }

            try
            {
                client.EndConnect(asyncResult);
            }
            catch when (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
