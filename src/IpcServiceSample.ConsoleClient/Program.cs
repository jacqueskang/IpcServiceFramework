using IpcServiceSample.ServiceContracts;
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
            Console.WriteLine("Invoking IpcService...");
            var client = new MyClient("pipeName");
            MyResponse response = await client.GetDataAsync(new MyRequest
            {
                Message = "Hello"
            }, iAmHandsome: true);

            Console.WriteLine($"Received response: {response.Message}");
        }
    }
}
