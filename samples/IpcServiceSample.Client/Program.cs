using IpcServiceSample.ServiceContracts;
using JKang.IpcServiceFramework.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace IpcServiceSample.ConsoleClient
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Type a phrase and press enter or press Ctrl+C to exit:");
                string input = Console.ReadLine();

                // register IPC clients
                ServiceProvider serviceProvider = new ServiceCollection()
                    .AddNamedPipeIpcClient<IInterProcessService>("client1", pipeName: "pipeinternal")
                    .BuildServiceProvider();

                // resolve IPC client factory
                IIpcClientFactory<IInterProcessService> clientFactory = serviceProvider
                    .GetRequiredService<IIpcClientFactory<IInterProcessService>>();

                // create client
                IIpcClient<IInterProcessService> client = clientFactory.CreateClient("client1");

                string output = await client.InvokeAsync(x => x.ReverseString(input));

                Console.WriteLine($"Result from server: '{output}'.\n");
            }
        }
    }
}
