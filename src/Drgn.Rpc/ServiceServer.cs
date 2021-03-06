using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Drgn.Rpc
{
    public class ServiceServer
    {
        private class Connection
        {
            public Connection(int id, ITransport procedureTransport)
            {
                ID = id;
                Info = procedureTransport.Info;
                ProcedureTransport = procedureTransport;
            }

            public int ID
            {
                private set; get;
            }

            public string Info {
                private set; get;
            }

            public bool Connected
            {
                get
                {
                    return ProcedureTransport.Connected && EventTransport != null && EventTransport.Connected;
                }
            }

            public ITransport ProcedureTransport;
            public Task? ConnectionLoopTask;
            public ITransport? EventTransport;
        }

        private IServiceDisptacher _dispatcher;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private TcpListener _listener;
        private Task _listenerTask;
        private ConcurrentDictionary<int, Connection> _clients = new ConcurrentDictionary<int, Connection>();
        private int nextConnectionID = 0;

        public delegate void ClientHandler(string connectionInfo);
        public event ClientHandler? OnClientConnection;
        public event ClientHandler? OnClientDisconnection;

        public ServiceServer(IServiceDisptacher dispatcher, int port)
        {
            _dispatcher = dispatcher;
            _dispatcher.OnEvent += eventDispatcher => Broadcast(eventDispatcher);
            _listener = new TcpListener(IPAddress.Any, port);
            _listenerTask = Task.Run(async () => await ListenerLoop(port));
        }

        private Connection AddConnection(TcpTransport procedureTransport)
        {
            var connection = new Connection(nextConnectionID, procedureTransport);
            nextConnectionID += 1;
            OnClientConnection?.Invoke(connection.Info);
            var result = _clients.TryAdd(connection.ID, connection);
            Debug.Assert(result);
            return connection;
        }

        private void Broadcast(EventDispatcher eventDispatcher)
        {
            foreach (var connection in _clients.Values)
            {
                if (connection.Connected && connection.EventTransport != null)
                {
                    eventDispatcher(connection.EventTransport);
                }
            }
        }

        private async Task ConnectionLoop(Connection connection) {
            var transport = connection.ProcedureTransport;
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

            connection.ProcedureTransport.Close();
            connection.EventTransport?.Close();
            _clients.TryRemove(connection.ID, out var _);
            OnClientDisconnection?.Invoke(connection.Info);
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
                    var message = await transport.ReadMessageType();
                    if (message == Message.Type.InitProcedureTransport)
                    {
                        var connection = AddConnection(transport);
                        await transport.WriteMessageType(Message.Type.AckProcedureTransport);
                        await transport.WriteInt32(connection.ID);
                    }
                    else if (message == Message.Type.InitEventTransport)
                    {
                        var id = await transport.ReadInt32();
                        if (_clients.TryGetValue(id, out var connection))
                        {
                            connection.EventTransport = transport;
                            await transport.WriteMessageType(Message.Type.AckEventTransport);
                            connection.ConnectionLoopTask = Task.Run(async () => await ConnectionLoop(connection));
                        }
                        else
                        {
                            Console.WriteLine("Event transport hand shake failure: connection does not exist");
                            transport.Close();
                        }
                    }

                }
                catch (SocketException e)
                {
                    if (e.ErrorCode != 995)
                    {
                        throw;
                    }
                }
            }
            Task.WaitAll(_clients.Select(x => x.Value.ConnectionLoopTask).OfType<Task>().ToArray());
        }

        public async Task StopAsync()
        {
            _listener.Stop();
            cancellationTokenSource.Cancel();
            await _listenerTask;
        }
    }
}
