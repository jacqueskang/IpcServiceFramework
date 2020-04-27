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
            Console.WriteLine("Type a phrase and press enter:");
            string input = Console.ReadLine();

            Console.WriteLine("Invoking inter-process service...");
            IIpcClient<IInterProcessService> client = new ServiceCollection()
                .AddNamedPipeIpcClient<IInterProcessService>("pipeinternal")
                .BuildServiceProvider()
                .GetRequiredService<IIpcClient<IInterProcessService>>();
            string output = await client.InvokeAsync(x => x.ReverseString(input));

            Console.WriteLine($"Result from server: '{output}'");
            Console.WriteLine("Press any key to exit.");
        }
    }
}
