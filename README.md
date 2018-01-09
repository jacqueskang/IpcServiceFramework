[![Build Status](https://travis-ci.org/jacqueskang/IpcServiceFramework.svg?branch=develop)](https://travis-ci.org/jacqueskang/IpcServiceFramework)

# IpcServiceFramework

A .NET Core lightweight inter-process communication framework allowing invoking a service via named pipeline (in a similar way as WCF, which is currently unavailable for .NET Core).

Support using primitive or complexe types in service contract.

Support multi-threading on server side with configurable number of threads.

[ASP.NET Core Dependency Injection framework](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) friendly.

## Usage
 1. Create an interface as service contract and package it in an assembly to be shared between server and client.
 2. Implement the service and host it in an console or web applciation
 3. Invoke the service with framework provided proxy client

## Sample:

 - Service contract
```csharp
    public interface IComputingService
    {
        float AddFloat(float x, float y);
    }
```

 - Service implementation

```csharp
    class ComputingService : IComputingService
    {
        public float AddFloat(float x, float y)
        {
            return x + y;
        }
    }
```

 - Invoke the service from client process

```csharp
    var proxy = new IpcServiceClient<IComputingService>("pipeName");
    float result = await proxy.InvokeAsync(x => x.AddFloat(1.23f, 4.56f));
```

 - Host the service (Console application)

```csharp
   class Program
    {
        static void Main(string[] args)
        {
            // configure DI
            IServiceCollection services = ConfigureServices(new ServiceCollection());

            // run IPC service host
            IpcServiceHostBuilder
                .Buid("pipeName", services.BuildServiceProvider())
                .Run();
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            return services
                .AddIpc(options =>
                {
                    options.ThreadCount = 4;
                })
                .AddService<IComputingService, ComputingService>();
        }
    }
```

 - Host the service (Web application)

```csharp
    public class Program
    {
        public static void Main(string[] args)
        {
            IWebHost webHost = BuildWebHost(args);

            // run the IPC service host in a separated thread because it's blocking
            ThreadPool.QueueUserWorkItem(StartIpcService,
                webHost.Services.CreateScope().ServiceProvider);

            webHost.Run();
        }

        private static void StartIpcService(object state)
        {
            var serviceProvider = state as IServiceProvider;
            IpcServiceHostBuilder
                .Buid("pipeName", serviceProvider as IServiceProvider)
                .Run();
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
                .AddIpc(options =>
                {
                    options.ThreadCount = 4;
                })
                .AddService<IComputingService, ComputingService>()
                ;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
```

I'll publish NuGet packages later.

Any contributions or comments are welcome!

