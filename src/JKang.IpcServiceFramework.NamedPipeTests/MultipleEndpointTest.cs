using AutoFixture.Xunit2;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using JKang.IpcServiceFramework.Client;
using JKang.IpcServiceFramework.Hosting;
using JKang.IpcServiceFramework.NamedPipeTests.Fixtures;
using JKang.IpcServiceFramework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace JKang.IpcServiceFramework.NamedPipeTests
{
    public class MultipleEndpointTest
    {
        [Theory, AutoData]
        public async Task MultipleEndpoints(
            Mock<ITestService> service1,
            Mock<ITestService2> service2)
        {
            IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddScoped(x => service1.Object)
                        .AddScoped(x => service2.Object);
                })
                .ConfigureIpcHost(builder =>
                {
                    builder
                        .AddNamedPipeEndpoint<ITestService>("pipe1")
                        .AddNamedPipeEndpoint<ITestService2>("pipe2");
                })
                .Build();

            await host.StartAsync();

            ServiceProvider clientServiceProvider = new ServiceCollection()
                .AddNamedPipeIpcClient<ITestService>("client1", "pipe1")
                .AddNamedPipeIpcClient<ITestService2>("client2", "pipe2")
                .BuildServiceProvider();

            IIpcClient<ITestService> client1 = clientServiceProvider
                .GetRequiredService<IIpcClientFactory<ITestService>>()
                .CreateClient("client1");

#if !DISABLE_DYNAMIC_CODE_GENERATION
            await client1.InvokeAsync(x => x.ReturnVoid());
            service1.Verify(x => x.ReturnVoid(), Times.Once);
#endif

            var request = TestHelpers.CreateIpcRequest("ReturnVoid");
            await client1.InvokeAsync(request);
#if !DISABLE_DYNAMIC_CODE_GENERATION
            service1.Verify(x => x.ReturnVoid(), Times.Exactly(2));
#else
            service1.Verify(x => x.ReturnVoid(), Times.Once);
#endif

            IIpcClient<ITestService2> client2 = clientServiceProvider
                .GetRequiredService<IIpcClientFactory<ITestService2>>()
                .CreateClient("client2");

#if !DISABLE_DYNAMIC_CODE_GENERATION
            await client2.InvokeAsync(x => x.SomeMethod());
            service2.Verify(x => x.SomeMethod(), Times.Once);
#endif

            request = TestHelpers.CreateIpcRequest("SomeMethod");
            await client2.InvokeAsync(request);

#if !DISABLE_DYNAMIC_CODE_GENERATION
            service2.Verify(x => x.SomeMethod(), Times.Exactly(2));
#else
            service2.Verify(x => x.SomeMethod(), Times.Once);
#endif
        }
    }
}
