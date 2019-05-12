using AutoFixture.Xunit2;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace JKang.IpcServiceFramework.IntegrationTests
{
    public class EdgeCaseTest : IDisposable
    {
        private readonly CancellationTokenSource _cancellationToken;
        private readonly int _port;
        private readonly IpcServiceClient<ITestService> _client;

        public EdgeCaseTest()
        {
            // configure DI
            IServiceCollection services = new ServiceCollection()
                .AddIpc(builder => builder.AddNamedPipe().AddService<ITestService, TestService>());
            _port = new Random().Next(10000, 50000);
            IIpcServiceHost host = new IpcServiceHostBuilder(services.BuildServiceProvider())
                .AddTcpEndpoint<ITestService>(
                    name: Guid.NewGuid().ToString(),
                    ipEndpoint: IPAddress.Loopback,
                    port: _port)
                .Build();
            _cancellationToken = new CancellationTokenSource();
            host.RunAsync(_cancellationToken.Token);

            _client = new IpcServiceClientBuilder<ITestService>()
                .UseTcp(IPAddress.Loopback, _port)
                .Build();
        }

        [Fact]
        public async Task HugeMessage()
        {
            byte[] buffer = new byte[100000000]; // 100MB
            new Random().NextBytes(buffer);
            byte[] result = await _client.InvokeAsync(x => x.ReverseBytes(buffer));
        }

        public void Dispose()
        {
            _cancellationToken.Cancel();
        }
    }
}
