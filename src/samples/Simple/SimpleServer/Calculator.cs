using System;
using System.Threading.Tasks;


namespace Samples
{
    using Generated;

    public class Calculator : ICalculator
    {
        public ValueTask<int> AddOneAsync(int value) {
            UsageNotification?.Invoke($"A client wants to compute {value} + 1.");
            Console.WriteLine($"Compute {value} + 1");
            return new ValueTask<int>(value + 1);
        }

        public event UsageNotificationHandler? UsageNotification;
    }
}
