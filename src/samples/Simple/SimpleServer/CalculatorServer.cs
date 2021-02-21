using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Drgn.Rpc;

namespace Samples
{
    public class CalculatorServer : Calculator, IDisposable
    {
        private ServiceServer _server;

        public CalculatorServer()
        {
             _server = new ServiceServer(new Generated.CalculatorDispatcher(this), 27500);
        }

        public void Dispose()
        {
            _server.StopAsync().Wait();
        }
    }
}
