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

                IIpcClient<IInterProcessService> client = new ServiceCollection()
                    .AddNamedPipeIpcClient<IInterProcessService>("pipeinternal")
                    .BuildServiceProvider()
                    .GetRequiredService<IIpcClient<IInterProcessService>>();
                string output = await client.InvokeAsync(x => x.ReverseString(input));

                Console.WriteLine($"Result from server: '{output}'.\n");
            }
        }
    }
}
