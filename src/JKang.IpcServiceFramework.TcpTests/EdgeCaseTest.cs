using JKang.IpcServiceFramework.Client;
using JKang.IpcServiceFramework.Testing.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace JKang.IpcServiceFramework.TcpTests
{
    public class EdgeCaseTest
    {
        [Fact]
        public async Task ServerIsOff_Timeout()
        {
            int timeout = 1000; // 1s
            IIpcClient<ITestService> client = new ServiceCollection()
                .AddTcpIpcClient<ITestService>(options =>
                {
                    options.ServerIp = IPAddress.Loopback;
                    options.ServerPort = 39356; // assuming this port is not used
                    options.ConnectionTimeout = timeout;
                })
                .BuildServiceProvider()
                .GetRequiredService<IIpcClient<ITestService>>();

            var sw = Stopwatch.StartNew();
            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                string output = await client.InvokeAsync(x => x.StringType("abc"));
            });

            Assert.True(sw.ElapsedMilliseconds < timeout * 1.5); // makesure timeout works with marge
        }
    }
}
