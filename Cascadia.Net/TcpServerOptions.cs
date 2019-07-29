using System.Net;

namespace Cascadia.Net
{
    public class TcpServerOptions
    {
        public IPEndPoint LocalEndpoint { get; set; }
        public bool Echo { get; set; }
        public int MaxConnections { get; set; } = int.MaxValue;
        public bool EnableKeepAlive { get; set; }
    }
}