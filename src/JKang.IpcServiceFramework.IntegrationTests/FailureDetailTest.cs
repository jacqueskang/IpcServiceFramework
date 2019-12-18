using AutoFixture.Xunit2;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace JKang.IpcServiceFramework.IntegrationTests
{
    public class FailureDetailTest : IDisposable
    {
        private readonly CancellationTokenSource _cancellationToken;
        private readonly int _port;
        private readonly IpcServiceClient<ITestService> _client;

        public FailureDetailTest()
        {
            // configure DI
            IServiceCollection services = new ServiceCollection()
                .AddIpc(builder => builder.AddNamedPipe().AddService<ITestService, TestService>());
            _port = new Random().Next(10000, 50000);
            IIpcServiceHost host = new IpcServiceHostBuilder(services.BuildServiceProvider())
                .AddTcpEndpoint<ITestService>(
                    name: Guid.NewGuid().ToString(),
                    ipEndpoint: IPAddress.Loopback,
                    port: _port,
                    includeFailureDetailsInResponse: true)
                .Build();
            _cancellationToken = new CancellationTokenSource();
            host.RunAsync(_cancellationToken.Token);

            _client = new IpcServiceClientBuilder<ITestService>()
                .UseTcp(IPAddress.Loopback, _port)
                .Build();
        }

        [Fact]
        public async Task ThrowException()
        {
            try
            {
                await _client.InvokeAsync(x => x.ThrowException("This was forced"));
            }
            catch (IpcServerException ex)
            {
                Assert.Contains("This was forced", ex.Message);
                Assert.Contains(IpcServerException.ServerFailureDetails, ex.ToString());
            }
        }

        public void Dispose()
        {
            _cancellationToken.Cancel();
        }
    }
}
