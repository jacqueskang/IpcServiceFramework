namespace IpcServiceSample.ServiceContracts
{
    public interface IComputingService
    {
        float AddFloat(float x, float y);
        ComplexNumber AddComplexNumber(ComplexNumber x, ComplexNumber y);
        void DoNothing();
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
}
