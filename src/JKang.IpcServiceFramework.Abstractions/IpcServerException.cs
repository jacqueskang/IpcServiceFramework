using System;
using System.Runtime.Serialization;
using System.Text;

namespace JKang.IpcServiceFramework
{
    /// <summary>
    /// An exception that originated at the server.
    /// </summary>
    [Serializable]
    public class IpcServerException : InvalidOperationException
    {
        public const string ServerFailureDetails = "Server failure details:";
        public string FailureDetails { get; }

        public IpcServerException()
        {
        }

        public IpcServerException(string message, string failureDetails)
            : base(message)
        {
            FailureDetails = failureDetails;
        }

        public IpcServerException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected IpcServerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            FailureDetails = info.GetString("FailureDetails");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("FailureDetails", FailureDetails, typeof(string));
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(FailureDetails))
            {
                var sb = new StringBuilder();
                sb.AppendLine(base.ToString());
                sb.AppendLine();
                sb.AppendLine(ServerFailureDetails);
                sb.Append(FailureDetails);
                return sb.ToString();
            }

            return base.ToString();
        }
    }
}
