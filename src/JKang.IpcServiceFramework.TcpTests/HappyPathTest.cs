using AutoFixture.Xunit2;
using JKang.IpcServiceFramework.Client;
using JKang.IpcServiceFramework.Hosting;
using JKang.IpcServiceFramework.TcpTests.Fixtures;
using JKang.IpcServiceFramework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace JKang.IpcServiceFramework.TcpTests
{
    public class HappyPathTest : IClassFixture<IpcApplicationFactory<ITestService>>
    {
        private static readonly Random _rand = new Random();
        private readonly Mock<ITestService> _serviceMock = new Mock<ITestService>();
        private readonly IIpcClient<ITestService> _client;

        public HappyPathTest(IpcApplicationFactory<ITestService> factory)
        {
            int port = _rand.Next(10000, 50000);
            _client = factory
                .WithServiceImplementation(_ => _serviceMock.Object)
                .WithIpcHostConfiguration(hostBuilder =>
                {
                    hostBuilder.AddTcpEndpoint<ITestService>(IPAddress.Loopback, port);
                })
                .CreateClient((name, services) =>
                {
                    services.AddTcpIpcClient<ITestService>(name, IPAddress.Loopback, port);
                });
        }

        [Theory, AutoData]
        public async Task HappyPath(string input, string expected)
        {
#if !DISABLE_DYNAMIC_CODE_GENERATION

            _serviceMock
                .Setup(x => x.StringType(input))
                .Returns(expected);

            string actual = await _client
                .InvokeAsync(x => x.StringType(input));

            Assert.Equal(expected, actual);
#endif
        }

        [Theory, AutoData]
        public async Task HappyPath2(string input, string expected)
        {
            _serviceMock
                .Setup(x => x.StringType(input))
                .Returns(expected);

            var request = TestHelpers.CreateIpcRequest(typeof(ITestService), "StringType", new object[] { input });
            string actual = await _client
                .InvokeAsync<string>(request);

            Assert.Equal(expected, actual);

            // Execute the method again to validate the socket isn't improperly disposed
            actual = await _client
                .InvokeAsync(x => x.StringType(input));
        }
    }
}
