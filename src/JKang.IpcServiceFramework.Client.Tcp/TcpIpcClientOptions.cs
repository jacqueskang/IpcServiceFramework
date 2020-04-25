using System.Net;
using System.Net.Security;

namespace JKang.IpcServiceFramework.Client.Tcp
{
    public class TcpIpcClientOptions
    {
        public IPAddress ServerIp { get; set; }
        public int ServerPort { get; set; }
        public bool EnableSsl { get; set; }
        public string SslServerIdentity { get; set; }
        public RemoteCertificateValidationCallback SslValidationCallback { get; set; }
    }
}
