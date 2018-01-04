using System;
using System.Threading.Tasks;

namespace IpcServiceSample.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            var client = new MyClient("testpipe");
            var request = new MyRequest
            {
                Message = "Hello"
            };

            Console.WriteLine("Invoking IpcService...");
            MyResponse response = await client.GetDataAsync(request, true);

            Console.WriteLine($"Received response: {response.Message}");
        }
    }
}
