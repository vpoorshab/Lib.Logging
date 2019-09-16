using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Lib.Logging.Abstractions;

namespace Lib.AspNetCore.Logging
{
    public class InternalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly InternalExceptionHandlerOptions _options;
        private readonly ILogger _logger;
        private readonly Func<object, Task> _clearCacheHeadersDelegate;
        public InternalExceptionHandlerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IOptions<InternalExceptionHandlerOptions> options)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<InternalExceptionHandlerMiddleware>();
            _options = options.Value;
            _clearCacheHeadersDelegate = ClearCacheHeaders;
            if (_options.ExceptionHandler != null)
                return;
            if (_options.ExceptionHandlingPath == null)
                throw new InvalidOperationException($"{nameof(_options.ExceptionHandlingPath)} is null");

            _options.ExceptionHandler = _next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (InternalException ex1)
            {
                PathString originalPath = context.Request.Path;

                if (!ex1.ExceptionHandled)
                {
                    if (string.IsNullOrWhiteSpace(ex1.RequestId))
                        ex1.SetRequestId(context.TraceIdentifier);

                    var scopeContext = new ScopeContext();
                    scopeContext.WithName(nameof(InternalExceptionHandlerMiddleware));
                    scopeContext.WithRequestId(string.IsNullOrWhiteSpace(ex1.RequestId) ? context.TraceIdentifier : ex1.RequestId);
                    scopeContext.Push("HttpStatusCode", ex1.HttpStatusCode.ToString());
                    scopeContext.Push(InternalHelper.CreateExceptionScopeContext(null, context, ex1));
                    scopeContext.Push("Path", originalPath.Value);
                    using (scopeContext.BeginScope(_logger))
                    {

                        _logger.LogError(ex1, ex1.UserFriendlyMessage);
                        ex1.ExceptionHandled = true;

                    }
                    if (context.Response.HasStarted)
                    {
                        _logger.LogWarning("The response has already started, the error handler will not be executed.");
                        throw;
                    }
                }
                
                if (_options.ExceptionHandlingPath.HasValue)
                {
                    context.Request.Path = _options.ExceptionHandlingPath;
                }
                try
                {
                    SetupContextResponse(context, ex1, originalPath.Value);
                    await _options.ExceptionHandler(context);
                    return;
                }
                catch (Exception ex2)
                {
                    // Suppress secondary exceptions, re-throw the original.
                    _logger.LogError(0, ex2, "An exception was thrown attempting to execute the error handler.");
                }
                finally
                {
                    context.Request.Path = originalPath;
                }
                throw; // Re-throw the original if we couldn't handle it
            }
            catch (Exception ex)
            {
                PathString originalPath = context.Request.Path;
                if (_options.ExceptionHandlingPath.HasValue)
                {
                    context.Request.Path = _options.ExceptionHandlingPath;
                }

                var internalException = ex.ConvertToInternalException(context.TraceIdentifier);

                var scopeContext = new ScopeContext();
                scopeContext.WithName("Exception Handler of Request")
                .WithRequestId(context.TraceIdentifier)
                .Push("Path", originalPath.Value)
                .Push(InternalHelper.CreateExceptionScopeContext(null, context, internalException));
                
                using (scopeContext.BeginScope(_logger))
                {
                    _logger.LogError(internalException, internalException.UserFriendlyMessage);
                    internalException.ExceptionHandled = true;
                }
                
                // We can't do anything if the response has already started, just abort.
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("The response has already started, the error handler will not be executed.");
                    throw;
                }


                try
                {
                    SetupContextResponse(context, internalException, originalPath.Value);
                    await _options.ExceptionHandler(context);
                    return;
                }
                catch (Exception ex2)
                {
                    // Suppress secondary exceptions, re-throw the original.
                    _logger.LogError(0, ex2, "An exception was thrown attempting to execute the error handler.");
                }
                finally
                {
                    context.Request.Path = originalPath;
                }
                throw; // Re-throw the original if we couldn't handle it
            }
        }

        private void SetupContextResponse(HttpContext context, InternalException ex, string path)
        {

            context.Response.Clear();
            var exceptionHandlerFeature = new InternalExceptionHandlerFeature
            {
                Error = ex,
                Path = path
            };
            context.Features.Set<IInternalExceptionHandlerFeature>(exceptionHandlerFeature);
            context.Response.StatusCode = (int)ex.HttpStatusCode;
            context.Response.OnStarting(_clearCacheHeadersDelegate, context.Response);
        }

        private Task ClearCacheHeaders(object state)
        {
            var response = (HttpResponse)state;
            response.Headers[HeaderNames.CacheControl] = "no-cache";
            response.Headers[HeaderNames.Pragma] = "no-cache";
            response.Headers[HeaderNames.Expires] = "-1";
            response.Headers.Remove(HeaderNames.ETag);
            return Task.CompletedTask;
        }

    }
}
