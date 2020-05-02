using AutoFixture.Xunit2;
using JKang.IpcServiceFramework.Client;
using JKang.IpcServiceFramework.Hosting;
using JKang.IpcServiceFramework.NamedPipeTests.Fixtures;
using JKang.IpcServiceFramework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;


namespace JKang.IpcServiceFramework.NamedPipeTests
{
    public class ErrorTest : IClassFixture<IpcApplicationFactory<ITestService>>
    {
        private readonly Mock<ITestService> _serviceMock = new Mock<ITestService>();
        private readonly IpcApplicationFactory<ITestService> _factory;

        public ErrorTest(IpcApplicationFactory<ITestService> factory)
        {
            _factory = factory
                .WithServiceImplementation(_ => _serviceMock.Object);
        }

        [Theory, AutoData]
        public async Task Exception_ThrowWithoutDetails(string pipeName, string details)
        {
            _serviceMock.Setup(x => x.ThrowException())
                .Throws(new Exception(details));

            IIpcClient<ITestService> client = _factory
                .WithIpcHostConfiguration(hostBuilder =>
                {
                    hostBuilder.AddNamedPipeEndpoint<ITestService>(options =>
                    {
                        options.PipeName = pipeName;
                        options.IncludeFailureDetailsInResponse = false;
                    });
                })
                .CreateClient(services =>
                {
                    services.AddNamedPipeIpcClient<ITestService>(options =>
                    {
                        options.PipeName = pipeName;
                    });
                });

            IpcServerUserCodeException actual = await Assert.ThrowsAsync<IpcServerUserCodeException>(async () =>
            {
                await client.InvokeAsync(x => x.ThrowException());
            });

            Assert.Null(actual.FailureDetails);
        }

        [Theory, AutoData]
        public async Task Exception_ThrowWithDetails(string pipeName, string details)
        {
            _serviceMock.Setup(x => x.ThrowException())
                .Throws(new Exception(details));

            IIpcClient<ITestService> client = _factory
                .WithIpcHostConfiguration(hostBuilder =>
                {
                    hostBuilder.AddNamedPipeEndpoint<ITestService>(options =>
                    {
                        options.PipeName = pipeName;
                        options.IncludeFailureDetailsInResponse = true;
                    });
                })
                .CreateClient(services =>
                {
                    services.AddNamedPipeIpcClient<ITestService>(options =>
                    {
                        options.PipeName = pipeName;
                    });
                });

            IpcServerUserCodeException actual = await Assert.ThrowsAsync<IpcServerUserCodeException>(async () =>
            {
                await client.InvokeAsync(x => x.ThrowException());
            });

            Assert.Contains(details, actual.FailureDetails);
        }
    }
}
