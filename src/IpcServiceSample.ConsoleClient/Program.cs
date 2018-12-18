using IpcServiceSample.ServiceContracts;
using IpcServiceSample.ServiceContracts.Helpers;
using JKang.IpcServiceFramework;
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
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
                Console.WriteLine("Press any key to stop.");
                var source = new CancellationTokenSource();

                await Task.WhenAll(RunTestsAsync(source.Token), Task.Run(() =>
                {
                    Console.ReadKey();
                    Console.WriteLine("Cancelling...");
                    source.Cancel();
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static bool InsecureValidationCallback_TESTONLY(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            /* WARNING: Using certificate validation callback can be dangerous. Incorrect implementation can lead to serious security issues.
             * 
             * For example, unconditionally returning true in this function provides no security whatsoever against an attacker who can perform
             * man-in-the-middle attacks.
             * 
             * This function is used only for test purposes. It validates only that the correct server certificate is used by the server.
             * However, it does not validate the certificate chain or validate that the certificate common name matches the server domain name.
             * Do not use this example in a production application.
            */
            return certificate.GetCertHashString() == "FA54627C36D3DAEFF69E04B59120992305A7104F";
        }

        private static async Task RunTestsAsync(CancellationToken cancellationToken)
        {
            IpcServiceClient<IComputingService> computingClient = new IpcServiceClientBuilder<IComputingService>()
                .UseNamedPipe("pipeName")
                .Build();

            IpcServiceClient<ISystemService> systemClient = new IpcServiceClientBuilder<ISystemService>()
                .UseTcp(IPAddress.Loopback, 45684)
                .Build();

            IpcServiceClient<ITestService> secureClient = new IpcServiceClientBuilder<ITestService>()
                .UseTcp(IPAddress.Loopback, 44384, "test-ipcsf-secure-server", InsecureValidationCallback_TESTONLY)
                .Build();

            IpcServiceClient<ITestService> xorTranslatedClient = new IpcServiceClientBuilder<ITestService>()
                .UseTcp(IPAddress.Loopback, 45454, s => new XorStream(s))
                .Build();

            IpcServiceClient<ISystemService> loggedClient = new IpcServiceClientBuilder<ISystemService>()
                .UseTcp(IPAddress.Loopback, 45684, s => new LoggingStream(s, "ipc.log"))
                .Build();

            // test 1: call IPC service method with primitive types
            float result1 = await computingClient.InvokeAsync(x => x.AddFloat(1.23f, 4.56f), cancellationToken);
            Console.WriteLine($"[TEST 1] sum of 2 floating number is: {result1}");

            // test 2: call IPC service method with complex types
            ComplexNumber result2 = await computingClient.InvokeAsync(x => x.AddComplexNumber(
                new ComplexNumber(0.1f, 0.3f),
                new ComplexNumber(0.2f, 0.6f)), cancellationToken);
            Console.WriteLine($"[TEST 2] sum of 2 complexe number is: {result2.A}+{result2.B}i");

            // test 3: call IPC service method with an array of complex types
            ComplexNumber result3 = await computingClient.InvokeAsync(x => x.AddComplexNumbers(new[]
            {
                new ComplexNumber(0.5f, 0.4f),
                new ComplexNumber(0.2f, 0.1f),
                new ComplexNumber(0.3f, 0.5f),
            }), cancellationToken);
            Console.WriteLine($"[TEST 3] sum of 3 complexe number is: {result3.A}+{result3.B}i", cancellationToken);

            // test 4: call IPC service method without parameter or return
            await systemClient.InvokeAsync(x => x.DoNothing(), cancellationToken);
            Console.WriteLine($"[TEST 4] invoked DoNothing()");

            // test 5: call IPC service method with enum parameter
            string text = await systemClient.InvokeAsync(x => x.ConvertText("hEllO woRd!", TextStyle.Upper), cancellationToken);
            Console.WriteLine($"[TEST 5] {text}");

            // test 6: call IPC service method returning GUID
            Guid generatedId = await systemClient.InvokeAsync(x => x.GenerateId(), cancellationToken);
            Console.WriteLine($"[TEST 6] generated ID is: {generatedId}");

            // test 7: call IPC service method with byte array
            byte[] input = Encoding.UTF8.GetBytes("Test");
            byte[] reversed = await systemClient.InvokeAsync(x => x.ReverseBytes(input), cancellationToken);
            Console.WriteLine($"[TEST 7] reversed bytes are: {Convert.ToBase64String(reversed)}");

            // test 8: call IPC service method with generic parameter
            string print = await systemClient.InvokeAsync(x => x.Printout(DateTime.UtcNow), cancellationToken);
            Console.WriteLine($"[TEST 8] print out value: {print}");

            // test 9: call slow IPC service method 
            await systemClient.InvokeAsync(x => x.SlowOperation(), cancellationToken);
            Console.WriteLine($"[TEST 9] Called slow operation");

            // test 10: call async server method
            await computingClient.InvokeAsync(x => x.MethodAsync());
            Console.WriteLine($"[TEST 10] Called async method");

            // test 11: call async server function
            int sum = await computingClient.InvokeAsync(x => x.SumAsync(1, 1));
            Console.WriteLine($"[TEST 11] Called async function: {sum}");
          
            // test 12: call secure service method
            generatedId = await secureClient.InvokeAsync(x => x.GenerateId(), cancellationToken);
            Console.WriteLine($"[TEST 12] Called secure service method, generated ID is: {generatedId}");

            // test 13 call translated service method
            generatedId = await xorTranslatedClient.InvokeAsync(x => x.GenerateId(), cancellationToken);
            Console.WriteLine($"[TEST 13] Called translated service method, generated ID is: {generatedId}");
            
            // test 14: use a translated stream to log data to a text file
            generatedId = await loggedClient.InvokeAsync(x => x.GenerateId(), cancellationToken);
            Console.WriteLine($"[TEST 14] Called method using stream translator for logging, generated ID is: {generatedId}");
        }
    }
}
