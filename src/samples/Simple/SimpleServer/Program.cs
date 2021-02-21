using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var addr = IPAddress.Any;
            var listener = new TcpListener(addr, 27500);
            var cancellationTokenSource = new CancellationTokenSource();

            var listenerTask = Task.Run(async () =>
            {
                var clients = new List<Task>();
                cancellationTokenSource.Token.Register(() => listener.Stop());
                listener.Start();
                Console.WriteLine("Server has started, listening on port 27500");
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var client = await listener.AcceptTcpClientAsync();
                        client.NoDelay = true;
                        Console.WriteLine("A client has connected");

                        var t = Task.Run(async () =>
                        {
                            var stream = client.GetStream();
                            var rawBuffer = new byte[1024];
                            var buffer = new Memory<byte>(rawBuffer);
                            try
                            {
                                while (!cancellationTokenSource.Token.IsCancellationRequested)
                                {
                                    var count = await stream.ReadAsync(buffer, cancellationTokenSource.Token);
                                    var message = Encoding.UTF8.GetString(rawBuffer, 0, count);
                                    Console.WriteLine($"Receive Message \"{message}\".");
                                }
                            } 
                            catch (OperationCanceledException) {}
                            catch (System.IO.IOException) {}

                            client.Close();
                        });

                        clients.Add(t);
                    } catch (SocketException e) {
                        if (e.ErrorCode != 995)
                        {
                            throw;
                        }
                    }
                }
                Task.WaitAll(clients.ToArray());
            });

            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();
            listener.Stop();
            cancellationTokenSource.Cancel();
            await listenerTask;
            Console.WriteLine("Server has been stopped");
        }
    }
}
