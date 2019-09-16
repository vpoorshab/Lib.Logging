using Lib.Logging.Abstractions;

namespace Lib.AspNetCore.Logging
{
    public class InternalExceptionHandlerFeature : IInternalExceptionHandlerFeature
    {
        public string Path { get; set; }
        public InternalException Error { get; set; }
        }
}
