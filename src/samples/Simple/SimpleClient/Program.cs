using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Drgn.Rpc;

namespace Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new Generated.CalculatorClient(new TcpTransportFactory(IPAddress.Loopback, 27500));
            client.UsageNotification += msg => Console.WriteLine($"Receive Message: '{msg}'");
            int value = 7;
            do
            {
                Console.WriteLine($"Add one to {value}.");
                int result = await client.AddOneAsync(value);
                Console.WriteLine($"  -> {value} + 1 = {result}");

                value *= 2;
                Console.WriteLine($"Press 'Escape' to stop, otherwise try to do ({value} + 1)...");
                var keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    break;
                }

            } while (true);

            client.Dispose();
        }
    }
}
