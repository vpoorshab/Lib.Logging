using System;
using System.Collections;
using System.Net;
using System.Runtime.Serialization;

namespace Lib.Logging.Abstractions
{
    public class InternalException : Exception
    {
        private readonly Exception __innerException;

        public InternalException(Exception exception, string userFriendlyMessage, string exceptionMessage, string requestId) : base(string.IsNullOrWhiteSpace(exceptionMessage)
            ? "An exception occurred"
            : exceptionMessage)
        {
            RequestId = requestId;
            UserFriendlyMessage = userFriendlyMessage;
            ExceptionHandled = false;
            HttpStatusCode = HttpStatusCode.InternalServerError;
            __innerException = exception;
        }

        public InternalException() : this(null, string.Empty, string.Empty, "undefined") { }

        public InternalException(Exception exception) : this(exception, string.Empty, string.Empty, "undefined") { }

        public InternalException(Exception exception, string requestId) : this(exception, string.Empty, string.Empty, requestId) { }

        public InternalException(Exception exception, string userFriendlyMessage, string exceptionMessage) : this(exception, userFriendlyMessage, exceptionMessage, "undefined") { }

        public InternalException(string userFriendlyMessage, string exceptionMessage, string requestId) : this(null, userFriendlyMessage, exceptionMessage, requestId) { }

        public void SetRequestId(string requestId)
        {
            this.RequestId = requestId;
        }

        public string UserFriendlyMessage { get; set; }

        public string RequestId { get; private set; }

        public Exception OriginalException => __innerException;

        public bool ExceptionHandled { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }

        public override IDictionary Data
        {
            get
            {
                if (__innerException != null)
                {
                    return __innerException.Data;
                }
                else
                {
                    return base.Data;
                }
            }
        }

        public override Exception GetBaseException()
        {
            if (__innerException != null)
            {
                return __innerException.GetBaseException();
            }

            return this;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (__innerException != null)
            {
                __innerException.GetObjectData(info, context);
            }
            else
            {
                base.GetObjectData(info, context);
            }
            
        }

        public override string HelpLink
        {
            get
            {
                if (__innerException != null)
                {
                    return __innerException.HelpLink;
                }

                return base.HelpLink;
            }
            set
            {
                if (__innerException != null)
                {
                    __innerException.HelpLink = value;
                }
                else
                {
                    base.HelpLink = value;
                }
            }
        }

        public override string Source
        {
            get
            {
                if (__innerException != null)
                    return __innerException.Source;
                else
                {
                    return base.Source;
                }
            }
            set
            {
                if (__innerException != null)
                {
                    __innerException.Source = value;
                }
                else
                {
                    base.Source = value;
                }
            }
        }

        public override string StackTrace
        {
            get
            {
                if (__innerException != null)
                {
                    return __innerException.StackTrace;
                }
                else
                {
                    return base.StackTrace;
                }
            }
        }

        public override string Message
        {
            get
            {
                if (__innerException != null)
                    return __innerException.Message;
                return base.Message;
            }
        }

        public override string ToString()
        {
            if (__innerException != null)
            {
                return __innerException.ToString();
            }
            else
            {
                return base.ToString();
            }
        }
    }

}
