using AutoFixture.Xunit2;
using JKang.IpcServiceFramework.Client;
using JKang.IpcServiceFramework.Hosting;
using JKang.IpcServiceFramework.NamedPipeTests.Fixtures;
using JKang.IpcServiceFramework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace JKang.IpcServiceFramework.NamedPipeTests
{
    /// <summary>
    /// Validates the IPC pipeline is working end-to-end for a variety of method types.
    /// Tests both dynamically generated IpcRequests (via DispatchProxy) and statically generated ones.
    /// Tests using simple parameter types (IpcClentOptions.UseSimpleTypeNameAssemblyFormatHandling == true).
    /// </summary>
    /// <seealso cref="Xunit.IClassFixture{JKang.IpcServiceFramework.Testing.IpcApplicationFactory{JKang.IpcServiceFramework.NamedPipeTests.Fixtures.ITestService}}" />
    public class SimpleTypeNameContractTest : IClassFixture<IpcApplicationFactory<ITestService>>
    {
        private readonly Mock<ITestService> _serviceMock = new Mock<ITestService>();
        private readonly IIpcClient<ITestService> _client;

        public SimpleTypeNameContractTest(IpcApplicationFactory<ITestService> factory)
        {
            string pipeName = Guid.NewGuid().ToString();
            _client = factory
                .WithServiceImplementation(_ => _serviceMock.Object)
                .WithIpcHostConfiguration(hostBuilder =>
                {
                    hostBuilder.AddNamedPipeEndpoint<ITestService>(options =>
                    {
                        options.PipeName = pipeName;
                        options.IncludeFailureDetailsInResponse = true;
                    });
                })
                .CreateClient((name, services) =>
                {
                    services.AddNamedPipeIpcClient<ITestService>(name, (_, options) =>
                    {
                        options.UseSimpleTypeNameAssemblyFormatHandling = true;
                        options.PipeName = pipeName;
                    }
                    );
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

        [Theory, AutoData]
        public async Task LargeComplexTypeArray(Complex input, Complex expected)
        {
            IEnumerable<Complex> largeInput = Enumerable.Repeat(input, 1000);
            IEnumerable<Complex> largeExpected = Enumerable.Repeat(expected, 100);

            _serviceMock
                .Setup(x => x.ComplexTypeArray(largeInput))
                .Returns(largeExpected);

            IEnumerable<Complex> actual = await _client
                .InvokeAsync(x => x.ComplexTypeArray(largeInput));

            Assert.Equal(largeExpected, actual);
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
