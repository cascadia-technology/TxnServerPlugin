using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Cascadia.Net
{
    public class TcpServer : IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly TcpServerOptions _options;

        private readonly object _lock = new object();
        private int _connections;

        public event EventHandler<byte[]> BytesReceived;
        public event EventHandler<Exception> ErrorMessage;
        public event EventHandler<string> InfoMessage;

        public TcpServer(TcpServerOptions options)
        {
            _options = options;
        }

        public async void Start()
        {
            try
            {
                await StartListening();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected exception: " + ex.Message);
                Console.WriteLine(ex.ToString());
            }
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        private async Task StartListening()
        {
            var listening = false;
            var listener = new TcpListener(_options.LocalEndpoint);
            try
            {
                while (true)
                {
                    try
                    {
                        while (true)
                        {
                            if (_connections >= _options.MaxConnections)
                            {
                                listener.Stop();
                                listening = false;
                            }
                            else if (listening && listener.Pending())
                            {
                                break;
                            }
                            else if (!listening)
                            {
                                listener.Start();
                                listening = true;
                            }
                                
                            await Task.Delay(100, _cts.Token);
                        }

                        lock (_lock)
                        {
                            OnInfoMessage("Accepting new client connection");
                            _connections++;
                            OnInfoMessage($"Connected Clients: {_connections}, Limit: {_options.MaxConnections}");
                            var task = HandleTcpClient(listener.AcceptTcpClient());
                        }

                    }
                    catch (TaskCanceledException)
                    {
                        OnInfoMessage("Shutting down TcpListener");
                    }
                    catch (Exception e)
                    {
                        OnError(e);
                        await Task.Delay(1000, _cts.Token);
                    }

                    if (_cts.IsCancellationRequested) break;
                }
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                listener.Stop();
            }
        }

        private async Task HandleTcpClient(TcpClient tcpClient)
        {
            IPEndPoint remoteEndPoint = null;
            IPEndPoint localEndPoint = null;
            try
            {
                remoteEndPoint = tcpClient.Client.RemoteEndPoint as IPEndPoint;
                localEndPoint = tcpClient.Client.LocalEndPoint as IPEndPoint;
                if (!IsRemoteEndpointAllowed(remoteEndPoint)) throw new UnauthorizedAccessException($"Unauthorized connection from {remoteEndPoint.Address}:{remoteEndPoint.Port}");
                OnInfoMessage(
                    $"Receiving connection from {remoteEndPoint} on {localEndPoint}");
                using (tcpClient)
                using (var stream = tcpClient.GetStream())
                {
                    SetKeepAlive(tcpClient, _options.EnableKeepAlive, 10000, 10000);
                    var buffer = new byte[tcpClient.ReceiveBufferSize];
                    while (!_cts.IsCancellationRequested)
                    {
                        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, _cts.Token);
                        if (bytesRead == 0)
                        {
                            OnInfoMessage("Received 0 bytes which indicates a client disconnection");
                            break;
                        }

                        var data = new byte[bytesRead];
                        Array.Copy(buffer, data, bytesRead);
                        OnBytesReceived(data);
                        if (_options.Echo)
                        {
                            await stream.WriteAsync(data, 0, data.Length, _cts.Token);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                OnInfoMessage($"Closing TcpClient connection due to cancellation");
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                OnInfoMessage($"Client {remoteEndPoint} disconnected");
                lock (_lock)
                {
                    _connections--;
                    OnInfoMessage($"Connected Clients: {_connections}, Limit: {_options.MaxConnections}");
                }
            }
        }

        private bool IsRemoteEndpointAllowed(IPEndPoint remoteEndPoint)
        {
            if (_options.AllowedRemoteEndpoints.Any(r => r == "*")) return true;
            if (remoteEndPoint.Address.Equals(IPAddress.Loopback) || remoteEndPoint.Address.Equals(IPAddress.IPv6Loopback))
                return true;
            if (_options.AllowedRemoteEndpoints.Contains(remoteEndPoint.Address.ToString()))
                return true;
            return false;
        }

        private static void SetKeepAlive(TcpClient tcpClient, bool on, uint keepAliveTime , uint keepAliveInterval )
        {
            Console.WriteLine($"Setting KeepAlive to {(on ? "Enabled" : "Disabled")}");
            var size = Marshal.SizeOf(new uint());
            var inOptionValues = new byte[size * 3];
            BitConverter.GetBytes((uint)(on ? 1 : 0)).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(inOptionValues, size);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(inOptionValues, size * 2);
            tcpClient.Client.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }

        protected virtual void OnBytesReceived(byte[] e)
        {
            BytesReceived?.Invoke(this, e);
        }

        protected virtual void OnError(Exception e)
        {
            ErrorMessage?.Invoke(this, e);
        }

        protected virtual void OnInfoMessage(string e)
        {
            InfoMessage?.Invoke(this, e);
        }

        public void Dispose()
        {
            if (!_cts.IsCancellationRequested) _cts.Cancel();
            BytesReceived = null;
            ErrorMessage = null;
        }
    }
}
