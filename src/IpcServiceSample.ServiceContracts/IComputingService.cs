using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IpcServiceSample.ServiceContracts
{
    public interface IComputingService
    {
        float AddFloat(float x, float y);
        ComplexNumber AddComplexNumber(ComplexNumber x, ComplexNumber y);
        ComplexNumber AddComplexNumbers(IEnumerable<ComplexNumber> numbers);

        Task MethodAsync();
        Task<int> SumAsync(int x, int y);
    }

    public class ComplexNumber
    {
        public float A { get; set; }
        public float B { get; set; }

        public ComplexNumber(float a, float b)
        {
            A = a;
            B = b;
        }
    }

    public enum TextStyle
    {
        TitleCase,
        Upper
    }
}
