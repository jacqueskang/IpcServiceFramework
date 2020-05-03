using JKang.IpcServiceFramework.Client;
using JKang.IpcServiceFramework.NamedPipeTests.Fixtures;
using JKang.IpcServiceFramework.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;


namespace JKang.IpcServiceFramework.NamedPipeTests
{
    public class EdgeCaseTest : IClassFixture<IpcApplicationFactory<ITestService>>
    {
        private readonly IpcApplicationFactory<ITestService> _factory;

        public EdgeCaseTest(IpcApplicationFactory<ITestService> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ServerIsOff_Timeout()
        {
            int timeout = 1000; // 1s
            IIpcClient<ITestService> client = _factory
                .CreateClient((name, services) =>
                {
                    services.AddNamedPipeIpcClient<ITestService>(name, (_, options) =>
                    {
                        options.PipeName = "inexisted-pipe";
                        options.ConnectionTimeout = timeout;
                    });
                });

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
            IIpcClient<ITestService> client = _factory
                .CreateClient((name, services) =>
                {
                    services.AddNamedPipeIpcClient<ITestService>(name, (_, options) =>
                    {
                        options.PipeName = "inexisted-pipe";
                        options.ConnectionTimeout = Timeout.Infinite;
                    });
                });

            using var cts = new CancellationTokenSource();

            Task.WaitAll(
                Task.Run(async () =>
                {
                    await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                    {
                        await client.InvokeAsync(x => x.ReturnVoid(), cts.Token);
                    });
                }),
                Task.Run(() => cts.CancelAfter(1000)));
        }
    }
}
