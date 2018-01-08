[![Build Status](https://travis-ci.org/jacqueskang/IpcServiceFramework.svg?branch=develop)](https://travis-ci.org/jacqueskang/IpcServiceFramework)

# IpcServiceFramework

A .NET Core lightweight inter-process communication framework allowing invoking a service via named pipeline (in a similar way as WCF, which is currently unavailable for .NET Core).

Support using primitive or complexe types in service contract.

[ASP.NET Core Dependency Injection framework](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) friendly.

## Usage
 1. Create an interface as service contract (ideally package in a shared assembly)
 2. Implement a client proxy with help of abstract class provided by framework
 3. Implement the service and register in IoC container
 4. Host the service in an console or web applciation

## Sample:

1. Service contract
```csharp
    public interface IComputingService
    {
        ComplexNumber AddComplexNumber(ComplexNumber x, ComplexNumber y);
        float AddFloat(float x, float y);
    }
```

2. Client side

```csharp
	// implement proxy
    class ComputingServiceClient : IpcServiceClient<IComputingService>, IComputingService
    {
        public ComputingServiceClient(string pipeName)
            : base(pipeName)
        { }

        public ComplexNumber AddComplexNumber(ComplexNumber x, ComplexNumber y)
        {
            return Invoke<ComplexNumber>(nameof(AddComplexNumber), x, y);
        }

        public float AddFloat(float x, float y)
        {
            return Invoke<float>(nameof(AddFloat), x, y);
        }
    }
```

```csharp
	// invoke IPC service
    var client = new ComputingServiceClient("pipeName");
    float result1 = client.AddFloat(1.23f, 4.56f);
    ComplexNumber result2 = client.AddComplexNumber(new ComplexNumber(0.1f, 0.3f), new ComplexNumber(0.2f, 0.6f));
```

3. Server side

```csharp
	// service implementation
    public class ComputingService : IComputingService
    {
        private readonly ILogger<ComputingService> _logger;

        public ComputingService(ILogger<ComputingService> logger) // inject dependencies in constructor
        {
            _logger = logger;
        }

        public ComplexNumber AddComplexNumber(ComplexNumber x, ComplexNumber y)
        {
            _logger.LogInformation($"{nameof(AddComplexNumber)} called.");
            return new ComplexNumber(x.A + y.A, x.B + y.B);
        }

        public float AddFloat(float x, float y)
        {
            _logger.LogInformation($"{nameof(AddFloat)} called.");
            return x + y;
        }
    }
```

4. Hosting in Console application

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

5. Hosting in Web application

```csharp
    public class Program
    {
        public static void Main(string[] args)
        {
            IWebHost webHost = BuildWebHost(args);

            ThreadPool.QueueUserWorkItem(StartIpcService,
                webHost.Services.CreateScope().ServiceProvider);

            webHost.Run();
        }

        private static void StartIpcService(object state)
        {
            var serviceProvider = state as IServiceProvider;
            IpcServiceHostBuilder
                .Buid("pipeName", serviceProvider as IServiceProvider)
                .Start();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
```


```csharp
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddIpc()
                .AddService<IComputingService, ComputingService>()
                ;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
```

I'll publish a NuGet package soon.

Any contributions or comments are welcome!

