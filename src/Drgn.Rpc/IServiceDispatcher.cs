using System;
using System.Threading.Tasks;

namespace Drgn.Rpc
{
    public delegate void EventDispatcher(ITransport transport);
    public delegate void OnEventHandler(EventDispatcher eventWriter);


    public interface IServiceDisptacher
    {
        event OnEventHandler OnEvent;

        Task Dispatch(ITransport transport);
    }
}
