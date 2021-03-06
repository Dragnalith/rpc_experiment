using System;
using System.Text;
using System.Threading.Tasks;

namespace Drgn.Rpc
{
    public interface ITransport : IDisposable
    {
        bool Connected { get; }
        string Info { get; }

        Task WriteInt32(int value);
        Task<int> ReadInt32(); 
        Task WriteBytes(byte[] value);
        Task<byte[]> ReadBytes();
        void Close();
    }

    public static class ITransportExtensions
    {
        public static Task WriteMessageType(this ITransport transport, Message.Type type)
        {
            return transport.WriteInt32((int)type);
        }

        public static async Task<Message.Type> ReadMessageType(this ITransport transport)
        {
            return (Message.Type)await transport.ReadInt32();
        }
        public static Task WriteString(this ITransport transport, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            return transport.WriteBytes(bytes);
        }

        public static async Task<string> ReadString(this ITransport transport)
        {
            var bytes = await transport.ReadBytes();
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
