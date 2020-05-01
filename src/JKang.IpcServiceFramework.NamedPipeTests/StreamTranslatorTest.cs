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
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace JKang.IpcServiceFramework.NamedPipeTests
{
    public class StreamTranslatorTest : IClassFixture<IpcApplicationFactory<ITestService>>
    {
        private readonly ContractTestCase _testCase;

        public StreamTranslatorTest(IpcApplicationFactory<ITestService> factory)
        {
            string pipeName = Guid.NewGuid().ToString();
            var serviceMock = new Mock<ITestService>();
            IIpcClient<ITestService> client = factory
                .WithServiceImplementation(_ => serviceMock.Object)
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
            _testCase = new ContractTestCase(serviceMock, client);
        }

        [Theory, AutoData]
        public Task StreamTranslator_HappyPath(string input, string expected)
            => _testCase.StringType(input, expected);
    }
}
