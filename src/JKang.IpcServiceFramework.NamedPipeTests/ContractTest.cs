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
                        options.IncludeFailureDetailsInResponse = true;
                    });
                })
                .CreateClient((name, services) =>
                {
                    services.AddNamedPipeIpcClient<ITestService>(name, pipeName);
                });
        }

        [Theory, AutoData]
        public async Task PrimitiveTypes(bool a, byte b, sbyte c, char d, decimal e, double f, float g, int h, uint i,
           long j, ulong k, short l, ushort m, int expected)
        {
            _serviceMock
                .Setup(x => x.PrimitiveTypes(a, b, c, d, e, f, g, h, i, j, k, l, m))
                .Returns(expected);

#if !DISABLE_DYNAMIC_CODE_GENERATION
            int actual = await _client
                .InvokeAsync(x => x.PrimitiveTypes(a, b, c, d, e, f, g, h, i, j, k, l, m));

            Assert.Equal(expected, actual);
#endif

            var request = TestHelpers.CreateIpcRequest(typeof(ITestService), "PrimitiveTypes", a, b, c, d, e, f, g, h, i, j, k, l, m);
            int actual2 = await _client.InvokeAsync<int>(request);

            Assert.Equal(expected, actual2);
        }

        [Theory, AutoData]
        public async Task StringType(string input, string expected)
        {
            _serviceMock
                .Setup(x => x.StringType(input))
                .Returns(expected);

#if !DISABLE_DYNAMIC_CODE_GENERATION
            string actual = await _client
                .InvokeAsync(x => x.StringType(input));

            Assert.Equal(expected, actual);
#endif

            var request = TestHelpers.CreateIpcRequest(typeof(ITestService), "StringType", input);
            string actual2 = await _client.InvokeAsync<string>(request);

            Assert.Equal(expected, actual2);
        }

        [Theory, AutoData]
        public async Task ComplexType(Complex input, Complex expected)
        {
            _serviceMock.Setup(x => x.ComplexType(input)).Returns(expected);

#if !DISABLE_DYNAMIC_CODE_GENERATION
            Complex actual = await _client
                .InvokeAsync(x => x.ComplexType(input));

            Assert.Equal(expected, actual);
#endif

            var request = TestHelpers.CreateIpcRequest(typeof(ITestService), "ComplexType", input);
            var actual2 = await _client.InvokeAsync<Complex>(request);

            Assert.Equal(expected, actual2);

        }

        [Theory, AutoData]
        public async Task ComplexTypeArray(IEnumerable<Complex> input, IEnumerable<Complex> expected)
        {
            _serviceMock
                .Setup(x => x.ComplexTypeArray(input))
                .Returns(expected);

#if !DISABLE_DYNAMIC_CODE_GENERATION
            IEnumerable<Complex> actual = await _client
                .InvokeAsync(x => x.ComplexTypeArray(input));

            Assert.Equal(expected, actual);
#endif

            var request = TestHelpers.CreateIpcRequest(typeof(ITestService), "ComplexTypeArray", input);
            var actual2 = await _client.InvokeAsync<IEnumerable<Complex>>(request);

            Assert.Equal(expected, actual2);
        }

        [Theory, AutoData]
        public async Task LargeComplexTypeArray(Complex input, Complex expected)
        {
            IEnumerable<Complex> largeInput = Enumerable.Repeat(input, 1000);
            IEnumerable<Complex> largeExpected = Enumerable.Repeat(expected, 100);

            _serviceMock
                .Setup(x => x.ComplexTypeArray(largeInput))
                .Returns(largeExpected);

#if !DISABLE_DYNAMIC_CODE_GENERATION
            IEnumerable<Complex> actual = await _client
                .InvokeAsync(x => x.ComplexTypeArray(largeInput));

            Assert.Equal(largeExpected, actual);
#endif

            var request = TestHelpers.CreateIpcRequest(typeof(ITestService), "ComplexTypeArray", largeInput);
            var actual2 = await _client.InvokeAsync<IEnumerable<Complex>>(request);

            Assert.Equal(largeExpected, actual2);
        }

        [Fact]
        public async Task ReturnVoid()
        {
#if !DISABLE_DYNAMIC_CODE_GENERATION
            await _client.InvokeAsync(x => x.ReturnVoid());
#endif

            await _client.InvokeAsync(TestHelpers.CreateIpcRequest("ReturnVoid"));
        }

        [Theory, AutoData]
        public async Task DateTime(DateTime input, DateTime expected)
        {
            _serviceMock.Setup(x => x.DateTime(input)).Returns(expected);

#if !DISABLE_DYNAMIC_CODE_GENERATION
            DateTime actual = await _client
                .InvokeAsync(x => x.DateTime(input));

            Assert.Equal(expected, actual);
#endif

            var request = TestHelpers.CreateIpcRequest(typeof(ITestService), "DateTime", input);
            DateTime actual2 = await _client.InvokeAsync<DateTime>(request);

            Assert.Equal(expected, actual2);
        }

        [Theory, AutoData]
        public async Task EnumType(DateTimeStyles input, DateTimeStyles expected)
        {
            _serviceMock.Setup(x => x.EnumType(input)).Returns(expected);

#if !DISABLE_DYNAMIC_CODE_GENERATION
            DateTimeStyles actual = await _client
                .InvokeAsync(x => x.EnumType(input));

            Assert.Equal(expected, actual);
#endif

            var request = TestHelpers.CreateIpcRequest(typeof(ITestService), "EnumType", input);
            DateTimeStyles actual2 = await _client.InvokeAsync<DateTimeStyles>(request);

            Assert.Equal(expected, actual2);
        }

        [Theory, AutoData]
        public async Task ByteArray(byte[] input, byte[] expected)
        {
            _serviceMock.Setup(x => x.ByteArray(input)).Returns(expected);

#if !DISABLE_DYNAMIC_CODE_GENERATION
            byte[] actual = await _client
                .InvokeAsync(x => x.ByteArray(input));

            Assert.Equal(expected, actual);
#endif

            var request = TestHelpers.CreateIpcRequest(typeof(ITestService), "ByteArray", input);
            byte[] actual2 = await _client.InvokeAsync<byte[]>(request);

            Assert.Equal(expected, actual2);
        }

        [Theory, AutoData]
        public async Task GenericMethod(decimal input, decimal expected)
        {
            _serviceMock
                .Setup(x => x.GenericMethod<decimal>(input))
                .Returns(expected);

#if !DISABLE_DYNAMIC_CODE_GENERATION
            decimal actual = await _client
                .InvokeAsync(x => x.GenericMethod<decimal>(input));

            Assert.Equal(expected, actual);
#endif
        }

        [Theory, AutoData]
        public async Task AsyncMethod(int expected)
        {
            _serviceMock
                .Setup(x => x.AsyncMethod())
                .ReturnsAsync(expected);

#if !DISABLE_DYNAMIC_CODE_GENERATION
            int actual = await _client
                .InvokeAsync(x => x.AsyncMethod());

            Assert.Equal(expected, actual);
#endif

            var request = TestHelpers.CreateIpcRequest("AsyncMethod");
            int actual2 = await _client.InvokeAsync<int>(request);

            Assert.Equal(expected, actual2);
        }

        [Theory, AutoData]
        public async Task Abstraction(TestDto input, TestDto expected)
        {
            _serviceMock
                .Setup(x => x.Abstraction(It.Is<TestDto>(o => o.Value == input.Value)))
                .Returns(expected);

#if !DISABLE_DYNAMIC_CODE_GENERATION
            ITestDto actual = await _client.InvokeAsync(x => x.Abstraction(input));

            Assert.Equal(expected.Value, actual.Value);
#endif

            var request = TestHelpers.CreateIpcRequest(typeof(ITestService), "Abstraction", input);
            ITestDto actual2 = await _client.InvokeAsync<ITestDto>(request);

            Assert.Equal(expected.Value, actual2.Value);
        }
    }
}
