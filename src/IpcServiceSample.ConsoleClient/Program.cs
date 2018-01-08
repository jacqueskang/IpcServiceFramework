using IpcServiceSample.ServiceContracts;
using System;

namespace IpcServiceSample.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Invoking IpcService...");
                var client = new ComputingServiceClient("pipeName");

                float result1 = client.AddFloat(1.23f, 4.56f);
                Console.WriteLine($"sum of 2 floating number is: {result1}");

                client.DoNothing();
                ComplexNumber result2 = client.AddComplexNumber(
                    new ComplexNumber(0.1f, 0.3f),
                    new ComplexNumber(0.2f, 0.6f));
                Console.WriteLine($"sum of 2 complexe number is: {result2.A}+{result2.B}i");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
