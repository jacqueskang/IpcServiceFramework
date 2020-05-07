using AutoFixture.Xunit2;
using JKang.IpcServiceFramework.Client;
using JKang.IpcServiceFramework.Hosting;
using JKang.IpcServiceFramework.Serialization.Protobuf.Tests.Fixtures;
using JKang.IpcServiceFramework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace JKang.IpcServiceFramework.Serialization.Protobuf.Tests
{
    public class ContractTest : IClassFixture<IpcApplicationFactory<ITestService>>
    {
        private readonly Mock<ITestService> _serviceMock = new Mock<ITestService>();
        private readonly IIpcClient<ITestService> _client;

        public ContractTest(IpcApplicationFactory<ITestService> factory)
        {
            string pipeName = Guid.NewGuid().ToString();
            _client = factory
                .WithServiceImplementation(_ => _serviceMock.Object)
                .WithIpcHostConfiguration(hostBuilder =>
                {
                    hostBuilder.AddNamedPipeEndpoint<ITestService>(options =>
                    {
                        options.PipeName = pipeName;
                        options.Serializer = new ProtobufIpcMessageSerializer();
                    });
                })
                .CreateClient((name, services) =>
                {
                    services.AddNamedPipeIpcClient<ITestService>(name, (_, options) =>
                    {
                        options.PipeName = pipeName;
                        options.Serializer = new ProtobufIpcMessageSerializer();
                    });
                });
        }

        [Theory, AutoData]
        public async Task Method1(int input, string expected)
        {
            _serviceMock
                .Setup(x => x.Method1(input))
                .Returns(expected);

            string actual = await _client
                .InvokeAsync(x => x.Method1(input));

            Assert.Equal(expected, actual);
        }
    }
}
