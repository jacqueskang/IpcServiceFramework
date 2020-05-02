using JKang.IpcServiceFramework.Client;
using JKang.IpcServiceFramework.TcpTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace JKang.IpcServiceFramework.TcpTests
{
    public class EdgeCaseTest
    {
        [Fact]
        public async Task ConnectionTimeout_Throw()
        {
            int timeout = 3000; // 3s
            IIpcClient<ITestService> client = new ServiceCollection()
                .AddTcpIpcClient<ITestService>(options =>
                {
                    // Connect to a non-routable IP address can trigger timeout
                    options.ServerIp = IPAddress.Parse("10.0.0.0");
                    options.ConnectionTimeout = timeout;
                })
                .BuildServiceProvider()
                .GetRequiredService<IIpcClient<ITestService>>();

            var sw = Stopwatch.StartNew();
            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                string output = await client.InvokeAsync(x => x.StringType("abc"));
            });

            Assert.True(sw.ElapsedMilliseconds < timeout * 2); // makesure timeout works with marge
        }

        [Fact]
        public void ConnectionCancelled_Throw()
        {
            IIpcClient<ITestService> client = new ServiceCollection()
                .AddTcpIpcClient<ITestService>(options =>
                {
                    // Connect to a non-routable IP address can trigger timeout
                    options.ServerIp = IPAddress.Parse("10.0.0.0");
                })
                .BuildServiceProvider()
                .GetRequiredService<IIpcClient<ITestService>>();

            using var cts = new CancellationTokenSource();

            Task.WaitAll(
                Task.Run(async () =>
                {
                    await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                    {
                        await client.InvokeAsync(x => x.StringType(string.Empty), cts.Token);
                    });
                }),
                Task.Run(() => cts.CancelAfter(1000)));
        }
    }
}
