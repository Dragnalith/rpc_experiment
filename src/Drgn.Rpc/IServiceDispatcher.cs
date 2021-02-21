using System;
using System.Threading.Tasks;

namespace Drgn.Rpc
{
    public interface IServiceDisptacher
    {
        Task Dispatch(ITransport transport);
    }
}
