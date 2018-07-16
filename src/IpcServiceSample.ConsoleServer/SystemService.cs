using IpcServiceSample.ServiceContracts;
using System;
using System.Globalization;
using System.Linq;

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

        public byte[] ReverseBytes(byte[] input)
        {
            return input.Reverse().ToArray();
        }
    }
}
