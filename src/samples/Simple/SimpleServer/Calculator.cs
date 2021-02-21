using System;
using System.Threading.Tasks;

namespace Samples
{
    public class Calculator : Generated.ICalculator
    {
        public ValueTask<int> AddOneAsync(int value) {
            Console.WriteLine($"Compute {value} + 1");
            return new ValueTask<int>(value + 1);
        }
    }
}
