using Cascadia.Net;
using System;
using System.Net;
using System.Text;

namespace TcpServerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new TcpServerOptions() {Echo = true, LocalEndpoint = new IPEndPoint(IPAddress.Any, 5123), MaxConnections = 1, EnableKeepAlive = true};
            var server = new TcpServer(options);
            server.BytesReceived += (sender, bytes) => Console.WriteLine(Encoding.ASCII.GetString(bytes));
            server.Start();
            Console.WriteLine("Listening on TCP " + options.LocalEndpoint.Port);
            
            Console.ReadKey();
            Console.WriteLine("Stopping");
            server.Stop();
            Console.ReadKey();
        }
    }


}
