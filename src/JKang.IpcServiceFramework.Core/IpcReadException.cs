using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;

namespace JKang.IpcServiceFramework
{
    [Serializable]
    public class IpcReadException : IOException
    {
        public IpcReadException()
        {
        }

        public IpcReadException(string message)
            : base(message)
        {
        }

        public IpcReadException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public IpcReadException(string element, int expectedBytes, int readBytes)
            : base(string.Format(CultureInfo.InvariantCulture,"{0} length must be {1} but was {2}",
                element, expectedBytes, readBytes))
        {
            ExpectedBytes = expectedBytes;
            ReadBytes = readBytes;
        }

        protected IpcReadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            ExpectedBytes = info.GetInt32("ExpectedBytes");
            ReadBytes = info.GetInt32("ReadBytes");
        }

        public int ExpectedBytes { get; }
        public int ReadBytes { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("ExpectedBytes", ExpectedBytes, typeof(int));
            info.AddValue("ReadBytes", ReadBytes, typeof(int));
        }
    }
}
