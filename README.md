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
    public interface IComputingService
    {
        float Add(float x, float y);
    }
```

2. Client side

```csharp
    class ComputingServiceClient : IpcServiceClient<IComputingService>, IComputingService
    {
        public ComputingServiceClient(string pipeName)
            : base(pipeName)
        { }

        public float Add(float x, float y)
        {
            return Invoke<float>(nameof(Add), x, y);
        }
    }
```

3. Server side

```csharp
	// service implementation
    public class ComputingService : IComputingService
    {
        private readonly ILogger<ComputingService> _logger;

        public ComputingService(ILogger<ComputingService> logger)
        {
            _logger = logger;
        }

        public float Add(float x, float y)
        {
            _logger.LogInformation($"{nameof(Add)} called.");
            return x + y;
        }
    }
```

```csharp
	// hosting in Console application
   class Program
    {
        static void Main(string[] args)
        {
            // build service provider
            IServiceCollection services = ConfigureServices(new ServiceCollection());
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // configure console logging
            serviceProvider.GetRequiredService<ILoggerFactory>()
                .AddConsole(LogLevel.Debug);

            // TODO start IPC service host
            IpcServiceHostBuilder
                .Buid("pipeName", serviceProvider as IServiceProvider)
                .Start();

        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services
                .AddLogging();

            services
                .AddIpc()
                .AddService<IComputingService, ComputingService>()
                ;

            return services;
        }
    }
```

I'll publish a NuGet package soon.

Any contributions or comments are welcome!

