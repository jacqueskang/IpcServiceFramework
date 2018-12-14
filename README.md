[![Build Status](https://travis-ci.org/jacqueskang/IpcServiceFramework.svg?branch=develop)](https://travis-ci.org/jacqueskang/IpcServiceFramework)

# IpcServiceFramework

A .NET Core lightweight inter-process communication framework allowing invoking a service via named pipeline and/or TCP (in a similar way as WCF, which is currently unavailable for .NET Core). Secure communication over SSL is also supported.

Support using primitive or complexe types in service contract.

Support multi-threading on server side with configurable number of threads (named pipeline endpoint only).

[ASP.NET Core Dependency Injection framework](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) friendly.

## Usage
 1. Create an interface as service contract and package it in an assembly to be shared between server and client.
 2. Implement the service and host it in an console or web applciation
 3. Invoke the service with framework provided proxy client

## Downloads

IpcServiceFramework is available via NuGet:

 - [JKang.IpcServiceFramework.Server](https://www.nuget.org/packages/JKang.IpcServiceFramework.Server/)
 - [JKang.IpcServiceFramework.Client](https://www.nuget.org/packages/JKang.IpcServiceFramework.Client/)

## Quick Start:

### Step 1: Create service contract
```csharp
    public interface IComputingService
    {
        float AddFloat(float x, float y);
    }
```
_Note: This interface is ideally to be placed in a library assembly to be shared between server and client._

### Step 2: Implement the server

1. Create a console application with the following 2 NuGet packages installed:

```
> Install-Package Microsoft.Extensions.DependencyInjection
> Install-Package JKang.IpcServiceFramework.Server
```

2. Add an class that implements the service contract

```csharp
    class ComputingService : IComputingService
    {
        public float AddFloat(float x, float y)
        {
            return x + y;
        }
    }
```

3. Configure and run the server

```csharp
    class Program
    {
        static void Main(string[] args)
        {
            // configure DI
            IServiceCollection services = ConfigureServices(new ServiceCollection());

            // build and run service host
            new IpcServiceHostBuilder(services.BuildServiceProvider())
                .AddNamedPipeEndpoint<IComputingService>(name: "endpoint1", pipeName: "pipeName")
                .AddTcpEndpoint<IComputingService>(name: "endpoint2", ipEndpoint: IPAddress.Loopback, port: 45684)
                .Build()
                .Run();
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            return services
                .AddIpc(builder =>
                {
                    builder
                        .AddNamedPipe(options =>
                        {
                            options.ThreadCount = 2;
                        })
                        .AddService<IComputingService, ComputingService>();
                });
        }
    }
```
_Note: It's possible to host IPC service in web application, please check out the sample project *IpcServiceSample.WebServer*_

### Step 3: Invoke the service from client process

1. Install the following package in client application:
```bash
$ dotnet add package JKang.IpcServiceFramework.Client
```

2. Add reference to the assembly created in step 1 which contains `IComputingService` interface.

3. Invoke the server

```csharp
    IpcServiceClient<IComputingService> client = new IpcServiceClientBuilder<IComputingService>()
        .UseNamedPipe("pipeName") // or .UseTcp(IPAddress.Loopback, 45684) to invoke using TCP
        .Build();

    float result = await client.InvokeAsync(x => x.AddFloat(1.23f, 4.56f));
```

__Welcome to raise any issue or even provide any suggestion/PR to participate this project!__

## Security

If you are running IPC channels over TCP on an untrusted network, you should consider using SSL. IpcServiceFramework supports SSL on TCP clients and hosts.

### Generate certificates for testing

**Do not use the provided certificates in the project folder.** These are used for example purposes only.

For testing, you can generate a self-signed certificate using the following openssl command:

    openssl req -x509 -newkey rsa:4096 -nodes -keyout key.pem -out cert.cer -days 365

This generates a key and a certificate that can be used for testing.

### Setting up the SSL endpoint

The endpoint requires a PKCS12 file containing both the certificate and a corresponding private key.

A certificate and key can be combined to a PKCS12 file for use with the server using the following command:

    openssl pkcs12 -export -in cert.cer -inkey key.pem -out server.pfx

You will be asked for a password.

You can import the certificate and provide it to the server endpoint using code similar to the following:

    var certificate = new X509Certificate2(@"path\to\server.pfx", "password");
	serviceHostBuilder.AddTcpEndpoint<ISomeServiceContract>("someEndpoint", ip, port, certificate);

See the ConsoleServer and WebServer projects for more complete examples.

Note: for security and maintenance reasons, we do not recommend that you hard-code the certificate password. It should instead be stored in the application configuration file so that it can be easily changed.

### Safe usage

SSL/TLS is only secure if you use it properly. Here are some tips:

* For production purposes, use a proper server certificate, signed by a real certificate authority (CA) or your organisation's internal CA. Do not use self-signed certificates in production.
* Do not use custom certificate validation callbacks on the client. They are hard to implement correctly and tend to result in security issues.
* Unconditionally returning true in a validation callback provides no security whatsoever against an attacker who can perform man-in-the-middle attacks.
* The callback used in the ConsoleServer project example is not secure. It checks for the correct certificate by hash but does not check its validity, expiry date, revocation status, or other important security properties.

### Client certificates

Client certificates are not currently supported.

## Stream translators

If you want to process the binary data after serialisation or before deserialisation, for example to add a custom handshake or packet header, you can do so using a stream translator. Host and client classes allow you to pass a `Func<Stream, Stream>` stream translation callback in their constructors, which can be used to "wrap" a custom stream around the network stream. This is supported on TCP communications both with and without SSL enabled. See the `XorStream` class in the IpcServiceSample.ServiceContracts project for an example of a stream translator.