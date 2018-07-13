using IpcServiceSample.ServiceContracts;
using JKang.IpcServiceFramework;
using System;
using System.Net;
using System.Threading.Tasks;

namespace IpcServiceSample.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        private static async Task MainAsync(string[] args)
        {
            try
            {
                var client = new TcpIpcServiceClient<IComputingService>(IPAddress.Parse("127.0.0.1"), 4578);

                // test 1: call IPC service method with primitive types
                float result1 = await client.InvokeAsync(x => x.AddFloat(1.23f, 4.56f));
                Console.WriteLine($"sum of 2 floating number is: {result1}");

                // test 2: call IPC service method with complex types
                ComplexNumber result2 = await client.InvokeAsync(x => x.AddComplexNumber(
                    new ComplexNumber(0.1f, 0.3f),
                    new ComplexNumber(0.2f, 0.6f)));
                Console.WriteLine($"sum of 2 complexe number is: {result2.A}+{result2.B}i");

                // test 3: call IPC service method with an array of complex types
                ComplexNumber result3 = await client.InvokeAsync(x => x.AddComplexNumbers(new[]
                {
                    new ComplexNumber(0.5f, 0.4f),
                    new ComplexNumber(0.2f, 0.1f),
                    new ComplexNumber(0.3f, 0.5f),
                }));
                Console.WriteLine($"sum of 3 complexe number is: {result3.A}+{result3.B}i");

                // test 4: call IPC service method without parameter or return
                await client.InvokeAsync(x => x.DoNothing());
                Console.WriteLine($"invoked DoNothing()");

                // test 5: call IPC service method with enum parameter
                string text = await client.InvokeAsync(x => x.ConvertText("hEllO woRd!", TextStyle.Upper));
                Console.WriteLine(text);

                // test 6: call IPC service method returning GUID
                Guid generatedId = await client.InvokeAsync(x => x.GenerateId());
                Console.WriteLine($"generated ID is: {generatedId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
