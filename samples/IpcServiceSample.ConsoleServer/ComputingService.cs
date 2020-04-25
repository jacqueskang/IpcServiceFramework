using IpcServiceSample.ServiceContracts;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IpcServiceSample.ConsoleServer
{
    public class ComputingService : IComputingService
    {
        private readonly ILogger<ComputingService> _logger;

        public ComputingService(ILogger<ComputingService> logger) // inject dependencies in constructor
        {
            _logger = logger;
        }

        public ComplexNumber AddComplexNumber(ComplexNumber x, ComplexNumber y)
        {
            _logger.LogInformation($"{nameof(AddComplexNumber)} called.");
            return new ComplexNumber(x.A + y.A, x.B + y.B);
        }

        public ComplexNumber AddComplexNumbers(IEnumerable<ComplexNumber> numbers)
        {
            _logger.LogInformation($"{nameof(AddComplexNumbers)} called.");
            var result = new ComplexNumber(0, 0);
            foreach (ComplexNumber number in numbers)
            {
                result = new ComplexNumber(result.A + number.A, result.B + number.B);
            }
            return result;
        }

        public float AddFloat(float x, float y)
        {
            _logger.LogInformation($"{nameof(AddFloat)} called.");
            return x + y;
        }

        public Task MethodAsync()
        {
            return Task.CompletedTask;
        }

        public Task<int> SumAsync(int x, int y)
        {
            return Task.FromResult(x + y);
        }
    }
}
