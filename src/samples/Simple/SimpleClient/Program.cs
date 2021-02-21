using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimpleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, 27500);
            client.NoDelay = true;
            var stream = client.GetStream();

            var message = $"My client time is {DateTime.Now}";
            var rawMessage = Encoding.UTF8.GetBytes(message);
            Console.WriteLine($"Client message is \"{Encoding.UTF8.GetString(rawMessage)}\".");
            await stream.WriteAsync(rawMessage, 0, rawMessage.Length);

            Console.WriteLine("Press any key to stop the client...");
            Console.ReadKey();
        }
    }
}
