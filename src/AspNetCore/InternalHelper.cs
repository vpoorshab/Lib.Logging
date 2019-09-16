using System;
using Lib.Logging.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Lib.AspNetCore.Logging
{
    internal class InternalHelper
    {
        public static ScopeContext CreateExceptionScopeContext(ActionDescriptor actionDescriptor, HttpContext httpContext, InternalException exception)
        {
            var scopeContext = new ScopeContext();
            if (actionDescriptor != null)
            {
                var controllerDescriptor = actionDescriptor as ControllerActionDescriptor;
                exception.Source = $"{controllerDescriptor?.DisplayName} -> {exception.Source}";
            }
            
            scopeContext.Push(nameof(exception.Source), exception.Source);
            scopeContext.Push(nameof(exception.HttpStatusCode), exception.HttpStatusCode.ToString());
            scopeContext.Push(nameof(exception.UserFriendlyMessage), $"{exception.UserFriendlyMessage}");
            scopeContext.Push(exception.Data);
            if (exception.InnerException != null)
            {
                scopeContext.Push(nameof(exception.InnerException.Source), exception.InnerException.Source);
                scopeContext.Push(exception.InnerException.Data);
                scopeContext.Push(nameof(exception.InnerException.Message), exception.InnerException?.Message);
                scopeContext.Push(nameof(exception.InnerException.StackTrace), exception.InnerException?.StackTrace);
            }

            return scopeContext;
        }

       
        public static InternalException ConvertToInternalException(string requestId, Exception exception)
        {
            return exception.ConvertToInternalException(requestId);
        }
    }
}
