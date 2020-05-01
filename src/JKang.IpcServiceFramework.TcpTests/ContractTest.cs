using AutoFixture.Xunit2;
using JKang.IpcServiceFramework.Client;
using JKang.IpcServiceFramework.Hosting;
using JKang.IpcServiceFramework.Testing;
using JKang.IpcServiceFramework.Testing.Fixtures;
using JKang.IpcServiceFramework.Testing.TestCases;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace JKang.IpcServiceFramework.TcpTests
{
    public class ContractTest : IClassFixture<IpcApplicationFactory<ITestService>>
    {
        private static readonly Random _rand = new Random();
        private readonly ContractTestCase _testCase;

        public ContractTest(IpcApplicationFactory<ITestService> factory)
        {
            int port = _rand.Next(10000, 50000);
            var serviceMock = new Mock<ITestService>();
            IIpcClient<ITestService> client = factory
                .WithServiceImplementation(_ => serviceMock.Object)
                .WithIpcHostConfiguration(hostBuilder =>
                {
                    hostBuilder.AddTcpEndpoint<ITestService>(IPAddress.Loopback, port);
                })
                .CreateClient(services =>
                {
                    services.AddTcpIpcClient<ITestService>(IPAddress.Loopback, port);
                });
            _testCase = new ContractTestCase(serviceMock, client);
        }

        [Theory, AutoData]
        public Task PrimitiveTypes(bool a, byte b, sbyte c, char d, decimal e, double f, float g, int h, uint i,
           long j, ulong k, short l, ushort m, int expected)
            => _testCase.PrimitiveTypes(a, b, c, d, e, f, g, h, i, j, k, l, m, expected);

        [Theory, AutoData]
        public Task StringType(string input, string expected)
            => _testCase.StringType(input, expected);

        [Theory, AutoData]
        public Task ComplexType(Complex input, Complex expected)
            => _testCase.ComplexType(input, expected);

        [Theory, AutoData]
        public Task ComplexTypeArray(IEnumerable<Complex> input, IEnumerable<Complex> expected)
            => _testCase.ComplexTypeArray(input, expected);

        [Fact]
        public Task ReturnVoid()
            => _testCase.ReturnVoid();

        [Theory, AutoData]
        public Task DateTime(DateTime input, DateTime expected)
            => _testCase.DateTime(input, expected);

        [Theory, AutoData]
        public Task EnumType(DateTimeStyles input, DateTimeStyles expected)
            => _testCase.EnumType(input, expected);

        [Theory, AutoData]
        public Task ByteArray(byte[] input, byte[] expected)
            => _testCase.ByteArray(input, expected);

        [Theory, AutoData]
        public Task GenericMethod(decimal input, decimal expected)
            => _testCase.GenericMethod(input, expected);

        [Theory, AutoData]
        public Task AsyncMethod(int expected)
            => _testCase.AsyncMethod(expected);

        [Theory, AutoData]
        public Task ThrowException(Exception expected)
            => _testCase.ThrowException(expected);
    }
}
