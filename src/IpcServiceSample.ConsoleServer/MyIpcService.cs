using IpcServiceSample.ServiceContracts;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IpcServiceSample.ConsoleServer
{
    public class MyIpcService : IMyIpcService
    {
        private readonly ILogger<MyIpcService> _logger;

        public MyIpcService(ILogger<MyIpcService> logger)
        {
            _logger = logger;
        }

        public Task<MyResponse> GetDataAsync(MyRequest request, bool iAmHandsome)
        {
            _logger.LogInformation($"{nameof(GetDataAsync)} called.");

            var response = new MyResponse
            {
                Message = $"What you said '{request.Message}' is {(iAmHandsome ? "correct." : "wrong")}"
            };
            return Task.FromResult(response);
        }
    }
}
