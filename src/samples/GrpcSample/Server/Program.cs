using System;
using System.Threading.Tasks;

using GrpcSample.Generated;

namespace GrpcSample
{
    class CalculatorImpl : Calculator.CalculatorBase
    {
        // Server side handler of the SayHello RPC
        public override Task<IntegerMessage> AddOne(IntegerMessage request, Grpc.Core.ServerCallContext context)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"Receive Request: Value = {request.Value}, Message = {request.Message}");
                return new IntegerMessage { Value = request.Value + 1, Message = request.Message };
            });
        }
    }

    class Program
    {
        const int Port = 30051;

        static async Task Main(string[] args)
        {
            Grpc.Core.Server server = new Grpc.Core.Server
            {
                Services = { Calculator.BindService(new CalculatorImpl()) },
                Ports = { new Grpc.Core.ServerPort("localhost", Port, Grpc.Core.ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Calculator server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            await server.ShutdownAsync();
        }
    }
}
