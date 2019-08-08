using System.Collections.Generic;
using System.Net;

namespace Cascadia.Net
{
    public class TcpServerOptions
    {
        public IPEndPoint LocalEndpoint { get; set; }
        public List<string> AllowedRemoteEndpoints { get; set; } = new List<string>();
        public bool Echo { get; set; }
        public int MaxConnections { get; set; } = int.MaxValue;
        public bool EnableKeepAlive { get; set; }
    }
}