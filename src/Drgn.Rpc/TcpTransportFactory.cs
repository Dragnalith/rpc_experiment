using System;
using System.Net;
using System.Threading;

namespace Drgn.Rpc
{
    public class TcpTransportFactory : ITransportFactory
    {
        private IPAddress _address;
        private int _port;

        public TcpTransportFactory(IPAddress address, int port)
        {
            _address = address;
            _port = port;
        }
        public ITransport Create(CancellationToken token)
        {
            return new TcpTransport(_address, _port, token);
        }
    }
}
