using Microsoft.AspNetCore.Http;

namespace Lib.AspNetCore.Logging
{
    public class InternalExceptionHandlerOptions 
    {
        public PathString ExceptionHandlingPath { get; set; }

        public RequestDelegate ExceptionHandler { get; set; }

    }
}