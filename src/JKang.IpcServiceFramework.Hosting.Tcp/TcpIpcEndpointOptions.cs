using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace JKang.IpcServiceFramework.Hosting.Tcp
{
    public class TcpIpcEndpointOptions : IpcEndpointOptions
    {
        public IPAddress IpEndpoint { get; set; } = IPAddress.Loopback;
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public X509Certificate SslCertificate { get; set; }
    }
}
