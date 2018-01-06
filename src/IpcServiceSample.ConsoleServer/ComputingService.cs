using IpcServiceSample.ServiceContracts;
using Microsoft.Extensions.Logging;

namespace IpcServiceSample.ConsoleServer
{
    public class ComputingService : IComputingService
    {
        private readonly ILogger<ComputingService> _logger;

        public ComputingService(ILogger<ComputingService> logger)
        {
            _logger = logger;
        }

        public float Add(float x, float y)
        {
            _logger.LogInformation($"{nameof(Add)} called.");
            return x + y;
        }
    }
}
