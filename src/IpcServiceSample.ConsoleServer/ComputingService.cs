using IpcServiceSample.ServiceContracts;
using Microsoft.Extensions.Logging;

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

        public float AddFloat(float x, float y)
        {
            _logger.LogInformation($"{nameof(AddFloat)} called.");
            return x + y;
        }
    }
}
