using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Drgn.Rpc
{
    public abstract class ServiceClient
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        protected ITransport _procedureTransport;
        protected ITransport _eventTransport;
        private Task _eventLoopTask;

        public ServiceClient(ITransportFactory factory)
        {
            _procedureTransport = factory.Create(_cancellationTokenSource.Token);
            _procedureTransport.WriteMessageType(Message.Type.InitProcedureTransport).Wait();
            var procedureAck = _procedureTransport.ReadMessageType().Result;
            if (Message.Type.AckProcedureTransport != procedureAck)
            {
                throw new InvalidOperationException($"The procedure transport failed hand shake. Received: '{procedureAck.ToString()}'");
            }

            var id = _procedureTransport.ReadInt32().Result;

            _eventTransport = factory.Create(_cancellationTokenSource.Token);
            _eventTransport.WriteMessageType(Message.Type.InitEventTransport).Wait();
            _eventTransport.WriteInt32(id).Wait();
            var eventAck = _eventTransport.ReadMessageType().Result;
            if (Message.Type.AckEventTransport != eventAck)
            {
                throw new InvalidOperationException($"The event transport failed hand shake. Received: '{eventAck.ToString()}'");
            }

            _eventLoopTask = Task.Run(async () => await EventLoop(_eventTransport));

        }

        private async Task EventLoop(ITransport transport)
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    int magicNumber = await transport.ReadInt32();
                    Debug.Assert(Message.MagicNumber == magicNumber);
                    Message.Type messageType = await transport.ReadMessageType();
                    Debug.Assert(Message.Type.Event == messageType);
                    await Dispatch(transport);
                }
            }
            catch (OperationCanceledException) { }
            catch (System.IO.IOException) { }

            transport.Dispose();
        }

        protected abstract Task Dispatch(ITransport transport);

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _eventLoopTask.Wait();
        }
    }
}
