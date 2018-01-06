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
            try
            {
                Console.WriteLine("Invoking IpcService...");
                var client = new ComputingServiceClient("pipeName");
                float x = 1.23f, y = 4.56f;
                float sum = client.Add(x, y);

                Console.WriteLine($"{x} + {y} = {sum}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
