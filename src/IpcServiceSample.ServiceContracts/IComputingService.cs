using System;
using System.Collections.Generic;

namespace IpcServiceSample.ServiceContracts
{
    public interface IComputingService
    {
        float AddFloat(float x, float y);
        ComplexNumber AddComplexNumber(ComplexNumber x, ComplexNumber y);
        ComplexNumber AddComplexNumbers(IEnumerable<ComplexNumber> numbers);
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
