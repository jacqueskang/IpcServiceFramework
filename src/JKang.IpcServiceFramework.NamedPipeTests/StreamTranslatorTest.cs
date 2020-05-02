using AutoFixture.Xunit2;
using JKang.IpcServiceFramework.Client;
using JKang.IpcServiceFramework.Hosting;
using JKang.IpcServiceFramework.NamedPipeTests.Fixtures;
using JKang.IpcServiceFramework.Testing;
using JKang.IpcServiceFramework.Testing.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace JKang.IpcServiceFramework.NamedPipeTests
{
    public class StreamTranslatorTest : IClassFixture<IpcApplicationFactory<ITestService>>
    {
        private readonly Mock<ITestService> _serviceMock = new Mock<ITestService>();
        private readonly IpcApplicationFactory<ITestService> _factory;

        public StreamTranslatorTest(IpcApplicationFactory<ITestService> factory)
        {
            _factory = factory;
        }

        [Theory, AutoData]
        public async Task StreamTranslator_HappyPath(string pipeName, string input, string expected)
        {
            _serviceMock
                .Setup(x => x.StringType(input))
                .Returns(expected);

            IIpcClient<ITestService> client = _factory
                .WithServiceImplementation(_ => _serviceMock.Object)
                .WithIpcHostConfiguration(hostBuilder =>
                {
                    hostBuilder.AddNamedPipeEndpoint<ITestService>(options =>
                    {
                        options.PipeName = pipeName;
                        options.StreamTranslator = x => new XorStream(x);
                    });
                })
                .CreateClient(services =>
                {
                    services.AddNamedPipeIpcClient<ITestService>(options =>
                    {
                        options.PipeName = pipeName;
                        options.StreamTranslator = x => new XorStream(x);
                    });
                });

            string actual = await client.InvokeAsync(x => x.StringType(input));

            Assert.Equal(expected, actual);
        }
    }
}
