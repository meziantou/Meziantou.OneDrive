using System;
using System.Net;
using System.Runtime.Serialization;

namespace Meziantou.OneDrive
{
    public class OneDriveException : Exception
    {
        public OneDriveException(string message) : base(message)
        { }

        public OneDriveException(string errorCode, string errorMessage, HttpStatusCode httpStatusCode, string httpReasonPhrase) : base(errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            HttpStatusCode = httpStatusCode;
            HttpReasonPhrase = httpReasonPhrase;
        }

        public OneDriveException(string message, string errorCode, string errorMessage, HttpStatusCode httpStatusCode, string httpReasonPhrase) : base(message)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            HttpStatusCode = httpStatusCode;
            HttpReasonPhrase = httpReasonPhrase;
        }

        public OneDriveException(string message, Exception innerException, string errorCode, string errorMessage, HttpStatusCode httpStatusCode, string httpReasonPhrase) : base(message, innerException)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            HttpStatusCode = httpStatusCode;
            HttpReasonPhrase = httpReasonPhrase;
        }

        protected OneDriveException(SerializationInfo info, StreamingContext context, string errorCode, string errorMessage, HttpStatusCode httpStatusCode, string httpReasonPhrase) : base(info, context)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            HttpStatusCode = httpStatusCode;
            HttpReasonPhrase = httpReasonPhrase;
        }

        public string ErrorCode { get; }
        public string ErrorMessage { get; }
        public HttpStatusCode HttpStatusCode { get; }
        public string HttpReasonPhrase { get; }

    }
}