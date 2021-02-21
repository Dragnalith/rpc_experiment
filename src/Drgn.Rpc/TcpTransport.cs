using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Drgn.Rpc
{
    public class TcpTransport : ITransport
    {
        private CancellationToken _cancellationToken;
        private TcpClient _client;
        private NetworkStream _stream;

        public TcpTransport(IPAddress address, int port, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _client = new TcpClient();
            cancellationToken.Register(() => _client.Close());
            _client.Connect(address, port);
            _stream = _client.GetStream();
        }

        public TcpTransport(TcpClient client, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _client = client;
            _stream = _client.GetStream();
            cancellationToken.Register(() => _client.Close());
        }

        public async Task WriteInt32(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            await _stream.WriteAsync(bytes, _cancellationToken);
        }

        public async Task<int> ReadInt32()
        {
            byte[] buffer = new byte[4];
            int totalCount = 0;
            while (totalCount < 4)
            {
                int count = await _stream.ReadAsync(buffer, totalCount , 4 - totalCount, _cancellationToken);
                totalCount += count;
            }
            Debug.Assert(totalCount == 4);

            return BitConverter.ToInt32(buffer);
        }

        public void Close()
        {
            _client.Close();
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
