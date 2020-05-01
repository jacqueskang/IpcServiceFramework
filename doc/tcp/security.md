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
