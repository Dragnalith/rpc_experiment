using System;
using System.Threading.Tasks;

namespace Drgn.Rpc
{
    public interface ITransport : IDisposable
    {
        Task WriteInt32(int value);
        Task<int> ReadInt32();
        void Close();
    }
}
