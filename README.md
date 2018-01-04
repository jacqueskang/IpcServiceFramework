# IpcServiceFramework

A .NET Core lightweight inter-process communication framework allowing invoking a service via named pipeline (in a similar way as WCF, which is currently unavailable for .NET Core).

[ASP.NET Core Dependency Injection framework](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) friendly.

## Usage
 1. Create an interface as service contract (ideally package in a shared assembly)
 2. On client side, implement a proxy with help of abstract class provided by framework
 3. On server side, implement the service and register in IoC container

## Sample:

1. Service contract
```csharp
using System.Threading.Tasks;

namespace IpcServiceSample.Contracts
{
    public interface IMyIpcService
    {
        Task<MyResponse> GetDataAsync(MyRequest request, bool iAmHandsome);
    }

    public class MyRequest
    {
        public string Message { get; set; }
    }

    public class MyResponse
    {
        public string Message { get; set; }
    }
}
```

2. Client side

```csharp
using IpcServiceSample.Contracts;
using JKang.IpcServiceFramework;
using System;
using System.Threading.Tasks;

namespace IpcServiceSample.ConsoleClient
{
    // implement proxy
    class MyClient : IpcServiceClient<IMyIpcService>, IMyIpcService
    {
        public MyClient(string pipeName)
            : base(pipeName)
        { }

        public Task<MyResponse> GetDataAsync(MyRequest request, bool iAmHandsome)
        {
            return InvokeAsync<MyResponse>(nameof(GetDataAsync), request, iAmHandsome);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            Console.WriteLine("Invoking IpcService...");
            var client = new MyClient("pipeName");
            MyResponse response = await client.GetDataAsync(new MyRequest
            {
                Message = "Hello"
            }, iAmHandsome: true);

            Console.WriteLine($"Received response: '{response.Message}'");
        }
    }
}
```

3. Server side

```csharp
using IpcServiceSample.Contracts;
using JKang.IpcServiceFramework;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace IpcServiceSample.ConsoleServer
{
    // service implementation
    public class MyIpcService : IMyIpcService
    {
        public Task<MyResponse> GetDataAsync(MyRequest request, bool iAmHandsome)
        {
            var response = new MyResponse
            {
                Message = $"What you said '{request.Message}' is {(iAmHandsome ? "correct." : "wrong")}"
            };
            return Task.FromResult(response);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // build service provider
            IServiceCollection services = ConfigureServices(new ServiceCollection());
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // start IPC service host
            IpcServiceHostBuilder
                .Buid("pipeName", serviceProvider as IServiceProvider)
                .Start();

        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            return services
                .AddScoped<IMyIpcService, MyIpcService>() // IoC registeration
                ;
        }
    }
}
```

I'll publish a NuGet package soon.

Any contributions or comments are welcome!

