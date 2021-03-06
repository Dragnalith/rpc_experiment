using System;
using System.Text;

namespace Drgn.Rpc
{
    public class ProtocolException : Exception
    {
        public ProtocolException(string message) 
            : base(message)
        {

        }
    }

    public static class Message
    {
        public static readonly int MagicNumber = BitConverter.ToInt32(Encoding.ASCII.GetBytes("DRPC"));
        
        public enum Type
        {
            Method = 0,
            Event = 1,
            InitProcedureTransport = 1,
            AckProcedureTransport = 2,
            InitEventTransport = 3,
            AckEventTransport = 4,
        }

    }
}