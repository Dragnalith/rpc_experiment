using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new CalculatorServer();

            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.Dispose();
        }
    }
}
