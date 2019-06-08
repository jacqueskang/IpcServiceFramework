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
    public class ContractTest : IDisposable
    {
        private readonly CancellationTokenSource _cancellationToken;
        private readonly int _port;
        private readonly IpcServiceClient<ITestService> _client;

        public ContractTest()
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

        [Theory, AutoData]
        public async Task SimpleType(float a, float b)
        {
            float actual = await _client.InvokeAsync(x => x.AddFloat(a, b));
            float expected = new TestService().AddFloat(a, b);
            Assert.Equal(expected, actual);
        }

        [Theory, AutoData]
        public async Task ComplexType(Complex a, Complex b)
        {
            Complex actual = await _client.InvokeAsync(x => x.AddComplex(a, b));
            Complex expected = new TestService().AddComplex(a, b);
            Assert.Equal(expected, actual);
        }

        [Theory, AutoData]
        public async Task ComplexTypeArray(IEnumerable<Complex> array)
        {
            Complex actual = await _client.InvokeAsync(x => x.SumComplexArray(array));
            Complex expected = new TestService().SumComplexArray(array);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ReturnVoid()
        {
            await _client.InvokeAsync(x => x.DoNothing());
        }

        [Theory, InlineData(" 2008-06-11T16:11:20.0904778Z", DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces)]
        public async Task EnumParameter(string value, DateTimeStyles styles)
        {
            DateTime actual = await _client.InvokeAsync(x => x.ParseDate(value, styles));
            DateTime expected = new TestService().ParseDate(value, styles);
            Assert.Equal(expected, actual);
        }

        [Theory, AutoData]
        public async Task ByteArray(byte[] input)
        {
            byte[] actual = await _client.InvokeAsync(x => x.ReverseBytes(input));
            byte[] expected = new TestService().ReverseBytes(input);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GenericParameter()
        {
            decimal actual = await _client.InvokeAsync(x => x.GetDefaultValue<decimal>());
            decimal expected = new TestService().GetDefaultValue<decimal>();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task AsyncOperation()
        {
            long actual = await _client.InvokeAsync(x => x.WaitAsync(500));
            Assert.True(actual >= 450);
        }

        [Fact]
        public async Task ExplicitInterfaceOperation()
        {
            int actual = await _client.InvokeAsync(x => x.ExplicitInterfaceMember());
            Assert.True(actual == 0);
        }

        public void Dispose()
        {
            _cancellationToken.Cancel();
        }
    }
}
