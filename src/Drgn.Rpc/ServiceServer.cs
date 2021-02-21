using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Drgn.Rpc
{
    public class ServiceServer
    {
        private IServiceDisptacher _dispatcher;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private TcpListener _listener;
        private Task _listenerTask;
        private List<Task> _clients = new List<Task>();

        private async Task ConnectionLoop(TcpTransport transport) {
            try
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    int magicNumber = await transport.ReadInt32();
                    Debug.Assert(Message.MagicNumber == magicNumber);
                    Message.Type messageType = (Message.Type) await transport.ReadInt32();
                    Debug.Assert(Message.Type.Method == messageType);
                    await _dispatcher.Dispatch(transport);
                }
            } 
            catch (OperationCanceledException) { }
            catch (System.IO.IOException) { }

            transport.Dispose();
        }

        private async Task ListenerLoop(int port)
        {
            cancellationTokenSource.Token.Register(() => _listener.Stop());
            _listener.Start();
            Console.WriteLine($"Server has started, listening on port {port}");
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    client.NoDelay = true;
                    var transport = new TcpTransport(client, cancellationTokenSource.Token);

                    var t = Task.Run(async () => await ConnectionLoop(transport));

                    _clients.Add(t);
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode != 995)
                    {
                        throw;
                    }
                }
            }
            Task.WaitAll(_clients.ToArray());
        }

        public ServiceServer(IServiceDisptacher dispatcher, int port)
        {
            _dispatcher = dispatcher;
            _listener = new TcpListener(IPAddress.Any, port);
            _listenerTask = Task.Run(async () => await ListenerLoop(port));
        }

        public async Task StopAsync()
        {
            _listener.Stop();
            cancellationTokenSource.Cancel();
            await _listenerTask;
        }
    }
}
