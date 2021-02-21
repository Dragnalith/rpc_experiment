using System;
using System.Threading;
using System.Threading.Tasks;
using Drgn.Rpc;

namespace Samples.Generated
{
    public interface ICalculator
    {
        ValueTask<int> AddOneAsync(int value);
    }

    public static class ICalculatorExtensions
    {
        public static int AddOne(this ICalculator calculator, int value) => calculator.AddOneAsync(value).Result;
    }

    public class CalculatorClient : ICalculator, IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private ITransport _transport;

        public  CalculatorClient(ITransportFactory factory)
        {
            _transport = factory.Create(_cancellationTokenSource.Token);
        }

        public async ValueTask<int> AddOneAsync(int value)
        {
            await _transport.WriteInt32(Message.MagicNumber);
            await _transport.WriteInt32((int) Message.Type.Method);
            await _transport.WriteInt32((int) CalculatorDispatcher.Method.AddOne);
            await _transport.WriteInt32(value);

            return await _transport.ReadInt32();
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }

    public class CalculatorDispatcher : IServiceDisptacher
    {
        public ICalculator _impl;

        public enum Method
        {
            AddOne = 0
        }

        public CalculatorDispatcher(ICalculator impl)
        {
            _impl = impl;
        }

        public async Task Dispatch(ITransport transport)
        {
            Method method = (Method) await transport.ReadInt32();
            switch (method)
            {
                case Method.AddOne:
                    int value = await transport.ReadInt32();
                    int result = await _impl.AddOneAsync(value);
                    await transport.WriteInt32(result);
                    break;
                default:
                    throw new ProtocolException("Unknown method identifier");
            }
        }
    }
}
