using AutoFixture.Xunit2;
using JKang.IpcServiceFramework.Client;
using JKang.IpcServiceFramework.Hosting;
using JKang.IpcServiceFramework.Testing;
using JKang.IpcServiceFramework.Testing.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace JKang.IpcServiceFramework.NamedPipeTests
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
                    hostBuilder.AddNamedPipeEndpoint<ITestService>(pipeName);
                })
                .CreateClient(services =>
                {
                    services.AddNamedPipeIpcClient<ITestService>(pipeName);
                });
        }

        [Theory, AutoData]
        public async Task PrimitiveTypes(bool a, byte b, sbyte c, char d, decimal e, double f, float g, int h, uint i,
           long j, ulong k, short l, ushort m, int expected)
        {
            _serviceMock
                .Setup(x => x.PrimitiveTypes(a, b, c, d, e, f, g, h, i, j, k, l, m))
                .Returns(expected);

            int actual = await _client
                .InvokeAsync(x => x.PrimitiveTypes(a, b, c, d, e, f, g, h, i, j, k, l, m));

            Assert.Equal(expected, actual);
        }

        [Theory, AutoData]
        public async Task StringType(string input, string expected)
        {
            _serviceMock
                .Setup(x => x.StringType(input))
                .Returns(expected);

            string actual = await _client
                .InvokeAsync(x => x.StringType(input));

            Assert.Equal(expected, actual);
        }

        [Theory, AutoData]
        public async Task ComplexType(Complex input, Complex expected)
        {
            _serviceMock.Setup(x => x.ComplexType(input)).Returns(expected);

            Complex actual = await _client
                .InvokeAsync(x => x.ComplexType(input));

            Assert.Equal(expected, actual);
        }

        [Theory, AutoData]
        public async Task ComplexTypeArray(IEnumerable<Complex> input, IEnumerable<Complex> expected)
        {
            _serviceMock
                .Setup(x => x.ComplexTypeArray(input))
                .Returns(expected);

            IEnumerable<Complex> actual = await _client
                .InvokeAsync(x => x.ComplexTypeArray(input));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ReturnVoid()
        {
            await _client.InvokeAsync(x => x.ReturnVoid());
        }

        [Theory, AutoData]
        public async Task DateTime(DateTime input, DateTime expected)
        {
            _serviceMock.Setup(x => x.DateTime(input)).Returns(expected);

            DateTime actual = await _client
                .InvokeAsync(x => x.DateTime(input));

            Assert.Equal(expected, actual);
        }

        [Theory, AutoData]
        public async Task EnumType(DateTimeStyles input, DateTimeStyles expected)
        {
            _serviceMock.Setup(x => x.EnumType(input)).Returns(expected);

            DateTimeStyles actual = await _client
                .InvokeAsync(x => x.EnumType(input));

            Assert.Equal(expected, actual);
        }

        [Theory, AutoData]
        public async Task ByteArray(byte[] input, byte[] expected)
        {
            _serviceMock.Setup(x => x.ByteArray(input)).Returns(expected);

            byte[] actual = await _client
                .InvokeAsync(x => x.ByteArray(input));

            Assert.Equal(expected, actual);
        }

        [Theory, AutoData]
        public async Task GenericMethod(decimal input, decimal expected)
        {
            _serviceMock
                .Setup(x => x.GenericMethod<decimal>(input))
                .Returns(expected);

            decimal actual = await _client
                .InvokeAsync(x => x.GenericMethod<decimal>(input));

            Assert.Equal(expected, actual);
        }

        [Theory, AutoData]
        public async Task AsyncMethod(int expected)
        {
            _serviceMock
                .Setup(x => x.AsyncMethod())
                .ReturnsAsync(expected);

            int actual = await _client
                .InvokeAsync(x => x.AsyncMethod());

            Assert.Equal(expected, actual);
        }

        [Theory, AutoData]
        public async Task Abstraction(TestDto input, TestDto expected)
        {
            _serviceMock
                .Setup(x => x.Abstraction(It.Is<TestDto>(o => o.Value == input.Value)))
                .Returns(expected);

            ITestDto actual = await _client.InvokeAsync(x => x.Abstraction(input));

            Assert.Equal(expected.Value, actual.Value);
        }
    }
}
