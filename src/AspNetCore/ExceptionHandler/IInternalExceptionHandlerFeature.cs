using Lib.Logging.Abstractions;

namespace Lib.AspNetCore.Logging
{
    public interface IInternalExceptionHandlerFeature
    {
        string Path { get; }
        InternalException Error { get; }

    }
}