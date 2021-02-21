using System;
using System.Threading;

namespace Drgn.Rpc
{
    public interface ITransportFactory
    {
        ITransport Create(CancellationToken token);
    }
}
