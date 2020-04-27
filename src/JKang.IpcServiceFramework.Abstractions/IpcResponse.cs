using System;
using System.Reflection;

namespace JKang.IpcServiceFramework
{
    public class IpcResponse
    {
        public IpcResponse(bool succeed, object data, string failure, string failureDetails, bool userCodeFailure)
        {
            Succeed = succeed;
            Data = data;
            Failure = failure;
            FailureDetails = failureDetails;
            UserCodeFailure = userCodeFailure;
        }

        public static IpcResponse Fail(string failure)
        {
            return new IpcResponse(false, null, failure, null, false);
        }

        public static IpcResponse Fail(Exception ex, bool includeDetails, bool userFailure = false)
        {
            if (ex is null)
            {
                throw new ArgumentNullException(nameof(ex));
            }

            string message = null;
            string details = null;

            if (!userFailure)
            {
                message = "Internal server error: ";
            }

            message += GetFirstUsableMessage(ex);

            if (includeDetails)
            {
                details = ex.ToString();
            }

            return new IpcResponse(false, null, message, details, userFailure);
        }

        public static IpcResponse Success(object data)
        {
            return new IpcResponse(true, data, null, null, false);
        }

        public bool Succeed { get; }
        public object Data { get; }
        public string Failure { get; }
        public string FailureDetails { get; }
        public bool UserCodeFailure { get; set; }

        public Exception GetException()
        {
            if (UserCodeFailure)
            {
                throw new IpcServerUserCodeException(Failure, FailureDetails);
            }

            throw new IpcServerException(Failure, FailureDetails);
        }

        private static string GetFirstUsableMessage(Exception ex)
        {
            Exception e = ex;

            while (e != null)
            {
                if (!(e is TargetInvocationException))
                {
                    return e.Message;
                }

                e = e.InnerException;
            }

            return ex.Message;
        }
    }
}
