using JKang.IpcServiceFramework.Client;
using JKang.IpcServiceFramework.Testing.Fixtures;
using Moq;
using Xunit;

namespace JKang.IpcServiceFramework.Testing
{
    public abstract class TestBase
        : IClassFixture<IpcApplicationFactory<ITestService>>
    {
        protected TestBase(IpcApplicationFactory<ITestService> factory)
        {
            Factory = factory.WithServiceImplementation(ServiceMock.Object);
        }

        protected Mock<ITestService> ServiceMock => new Mock<ITestService>();

        protected IpcApplicationFactory<ITestService> Factory { get; }

        protected abstract IIpcClient<ITestService> CreateClient(
            IpcApplicationFactory<ITestService> factory);
    }
}
