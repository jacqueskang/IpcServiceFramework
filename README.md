[![Build Status](https://dev.azure.com/jacques-kang/IpcServiceFramework/_apis/build/status/IpcServiceFramework%20CI?branchName=develop)](https://dev.azure.com/jacques-kang/IpcServiceFramework/_build/latest?definitionId=9&branchName=develop)

# IpcServiceFramework

A .NET Core 3.1 based lightweight framework for efficient inter-process communication.
Named pipeline and TCP support out-of-the-box, extensible with other protocols.

## Usage
 1. Create an interface as service contract and package it in an assembly to be shared between server and client.
 2. Implement the service and host it in an console or web applciation.
 3. Invoke the service with framework provided proxy client.

## Downloads

IpcServiceFramework is available via NuGet:

 - [JKang.IpcServiceFramework.Hosting.NamedPipe](https://www.nuget.org/packages/JKang.IpcServiceFramework.Hosting.NamedPipe/)
 - [JKang.IpcServiceFramework.Client.NamedPipe](https://www.nuget.org/packages/JKang.IpcServiceFramework.Client.NamedPipe/)
 - [JKang.IpcServiceFramework.Hosting.Tcp](https://www.nuget.org/packages/JKang.IpcServiceFramework.Hosting.Tcp/)
 - [JKang.IpcServiceFramework.Client.NamedPipe](https://www.nuget.org/packages/JKang.IpcServiceFramework.Client.NamedPipe/)

## Quick Start:

### Step 1: Create service contract
```csharp
    public interface IInterProcessService
    {
        string ReverseString(string input);
    }
```
_Note: This interface is ideally to be placed in a library assembly to be shared between server and client._

### Step 2: Implement the server

1. Create a console application with the following NuGet packages installed:

```
> Install-Package Microsoft.Extensions.Hosting
> Install-Package JKang.IpcServiceFramework.Hosting.NamedPipe
```

2. Add an class that implements the service contract

```csharp
    class InterProcessService : IInterProcessService
    {
        public string ReverseString(string input)
        {
            char[] charArray = input.ToCharArray();
            Array.Reverse(input.ToCharArray());
            return new string(charArray);
        }
    }
```

3. Configure and run the server

```csharp
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddScoped<IInterProcessService, InterProcessService>();
                })
                .ConfigureIpcHost(builder =>
                {
                    builder.AddNamedPipeEndpoint<IInterProcessService>("endpoint1", "pipeinternal");
                });
    }
```

### Step 3: Invoke the service from client process

1. Install the following package in client application:
```
> Install-Package JKang.IpcServiceFramework.Client.NamedPipe
```

2. Invoke the server

```csharp
    IIpcClient<IInterProcessService> client = new ServiceCollection()
	.AddNamedPipeIpcClient<IInterProcessService>("pipeinternal")
	.BuildServiceProvider()
	.GetRequiredService<IIpcClient<IInterProcessService>>();

    string output = await client.InvokeAsync(x => x.ReverseString(input));
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

If you want to process the binary data after serialisation or before deserialisation, for example to add a custom handshake when the connection begins, you can do so using a stream translator. Host and client classes allow you to pass a `Func<Stream, Stream>` stream translation callback in their constructors, which can be used to "wrap" a custom stream around the network stream. This is supported on TCP communications both with and without SSL enabled. See the `XorStream` class in the IpcServiceSample.ServiceContracts project for an example of a stream translator.

Stream translators are also useful for logging packets for debugging. See the `LoggingStream` class in the IpcServiceSample.ServiceContracts project for an example of using a stream translator to log traffic.
