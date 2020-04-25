using IpcServiceSample.ServiceContracts;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace IpcServiceSample.ConsoleServer
{
    public class SystemService : ISystemService
    {
        public string ConvertText(string text, TextStyle style)
        {
            switch (style)
            {
                case TextStyle.TitleCase:
                    return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text);
                case TextStyle.Upper:
                    return CultureInfo.InvariantCulture.TextInfo.ToUpper(text);
                default:
                    return text;
            }
        }

        public void DoNothing()
        { }

        public Guid GenerateId()
        {
            return Guid.NewGuid();
        }

        public string Printout<T>(T value)
        {
            return value.ToString();
        }

        public byte[] ReverseBytes(byte[] input)
        {
            return input.Reverse().ToArray();
        }

        public void SlowOperation()
        {
            Thread.Sleep(10000);
        }
    }
}
