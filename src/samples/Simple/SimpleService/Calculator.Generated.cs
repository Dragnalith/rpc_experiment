using System;
using System.Threading;
using System.Threading.Tasks;
using Drgn.Rpc;

namespace Samples.Generated
{
    public delegate void UsageNotificationHandler(string notificationMessage);
    
    public interface ICalculator
    {

        ValueTask<int> AddOneAsync(int value);

        event UsageNotificationHandler UsageNotification;
    }

    public static class ICalculatorExtensions
    {
        public static int AddOne(this ICalculator calculator, int value) => calculator.AddOneAsync(value).Result;
    }

    public class CalculatorClient : ServiceClient, ICalculator, IDisposable
    {
        public event UsageNotificationHandler UsageNotification;

        public  CalculatorClient(ITransportFactory factory)
            : base(factory)
        {
        }

        public async ValueTask<int> AddOneAsync(int value)
        {
            await _procedureTransport.WriteInt32(Message.MagicNumber);
            await _procedureTransport.WriteMessageType(Message.Type.Method);
            await _procedureTransport.WriteInt32((int) CalculatorDispatcher.Method.AddOne);
            await _procedureTransport.WriteInt32(value);

            return await _procedureTransport.ReadInt32();
        }
        public enum Event
        {
            UsageNotification = 0
        }

        protected override async Task Dispatch(ITransport transport)
        {
            Event method = (Event)await transport.ReadInt32();
            switch (method)
            {
                case Event.UsageNotification:
                    string value = await transport.ReadString();
                    UsageNotification?.Invoke(value);
                    break;
                default:
                    throw new ProtocolException("Unknown event identifier");
            }
        }
    }

    public class CalculatorDispatcher : IServiceDisptacher
    {
        private ICalculator _impl;
        public enum Method
        {
            AddOne = 0
        }

        public event OnEventHandler OnEvent;

        public CalculatorDispatcher(ICalculator impl)
        {
            _impl = impl;
            _impl.UsageNotification += OnUsageNotification;
        }

        private void OnUsageNotification(string value)
        {
            OnEvent?.Invoke(async transport => {
                await transport.WriteInt32(Message.MagicNumber);
                await transport.WriteMessageType(Message.Type.Event);
                await transport.WriteInt32((int)CalculatorClient.Event.UsageNotification);
                await transport.WriteString(value);
            });
        }

        public async Task Dispatch(ITransport transport)
        {
            Method method = (Method)await transport.ReadInt32();
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
