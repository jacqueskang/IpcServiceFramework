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

## Downloads

IpcServiceFramework is available via NuGet:

 - [JKang.IpcServiceFramework.Core](https://www.nuget.org/packages/JKang.IpcServiceFramework.Core/)
 - [JKang.IpcServiceFramework.Server](https://www.nuget.org/packages/JKang.IpcServiceFramework.Server/)
 - [JKang.IpcServiceFramework.Client](https://www.nuget.org/packages/JKang.IpcServiceFramework.Client/)

## Quick Start:

### Step 1 - Create service contract
```csharp
    public interface IComputingService
    {
        float AddFloat(float x, float y);
    }
```

### Step 2: Implement the service

```csharp
    class ComputingService : IComputingService
    {
        public float AddFloat(float x, float y)
        {
            return x + y;
        }
    }
```

### Step 3 - Host the service in Console application

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
It's possible to host IPC service in web application, please check out the sample project *IpcServiceSample.WebServer*

### Step 4 - Invoke the service from client process

```csharp
    var proxy = new IpcServiceClient<IComputingService>("pipeName");
    float result = await proxy.InvokeAsync(x => x.AddFloat(1.23f, 4.56f));
```

__Please feel free to download, fork and/or provide any feedback!__
