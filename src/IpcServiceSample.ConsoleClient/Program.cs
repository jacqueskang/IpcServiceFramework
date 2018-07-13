using IpcServiceSample.ServiceContracts;
using JKang.IpcServiceFramework;
using System;
using System.Net;
using System.Text;
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
                IpcServiceClient<IComputingService> client = new IpcServiceClientBuilder<IComputingService>()
                    // Comment in if you wish to use TCP instead
                    .UseNamedPipe("pipeName")
                    // Comment out if you wish to use TCP instead
                    // .UseTcp(IPAddress.Loopback, 45684)
                    .Build();

                // test 1: call IPC service method with primitive types
                float result1 = await client.InvokeAsync(x => x.AddFloat(1.23f, 4.56f));
                Console.WriteLine($"[TEST 1] sum of 2 floating number is: {result1}");

                // test 2: call IPC service method with complex types
                ComplexNumber result2 = await client.InvokeAsync(x => x.AddComplexNumber(
                    new ComplexNumber(0.1f, 0.3f),
                    new ComplexNumber(0.2f, 0.6f)));
                Console.WriteLine($"[TEST 2] sum of 2 complexe number is: {result2.A}+{result2.B}i");

                // test 3: call IPC service method with an array of complex types
                ComplexNumber result3 = await client.InvokeAsync(x => x.AddComplexNumbers(new[]
                {
                    new ComplexNumber(0.5f, 0.4f),
                    new ComplexNumber(0.2f, 0.1f),
                    new ComplexNumber(0.3f, 0.5f),
                }));
                Console.WriteLine($"[TEST 3] sum of 3 complexe number is: {result3.A}+{result3.B}i");

                // test 4: call IPC service method without parameter or return
                await client.InvokeAsync(x => x.DoNothing());
                Console.WriteLine($"[TEST 4] invoked DoNothing()");

                // test 5: call IPC service method with enum parameter
                string text = await client.InvokeAsync(x => x.ConvertText("hEllO woRd!", TextStyle.Upper));
                Console.WriteLine($"[TEST 5] {text}");

                // test 6: call IPC service method returning GUID
                Guid generatedId = await client.InvokeAsync(x => x.GenerateId());
                Console.WriteLine($"[TEST 6] generated ID is: {generatedId}");

                // test 7: call IPC service method with byte array
                byte[] input = Encoding.UTF8.GetBytes("Test");
                byte[] reversed = await client.InvokeAsync(x => x.ReverseBytes(input));
                Console.WriteLine($"[TEST 7] reversed bytes are: {Convert.ToBase64String(reversed)}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
