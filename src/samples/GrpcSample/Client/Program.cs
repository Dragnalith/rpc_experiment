using System;
using System.Threading.Tasks;

using GrpcSample.Generated;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Grpc.Core.Channel channel = new Grpc.Core.Channel("127.0.0.1:30051", Grpc.Core.ChannelCredentials.Insecure);

            var client = new Calculator.CalculatorClient(channel);

            var request = new IntegerMessage { Value = 56, Message = "Hello Grpc World" };
            Console.WriteLine($"Request: Value = {request.Value}, Message = {request.Message}");
            var reply = client.AddOne(request);
            Console.WriteLine($"Reply: Value = {reply.Value}, Message = {reply.Message}");

            await channel.ShutdownAsync();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
