using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lib.Logging.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;



namespace Lib.AspNetCore.Logging
{
    public class TraceActionFilter : IAsyncActionFilter, IAsyncResultFilter, IActionFilter, IResultFilter
    {


        #region Fields & Properties

        private const string LoggerCategoryTypeName = "Lib.AspNetCore.Logging.TraceAction.{0}";
        private const string ActionExecutionCategoryType = "ActionExecution";
        private const string ResultExecutionCategoryType = "ResultExecution";

        private readonly bool _logPostData;
        private readonly string[] _excludeFormDataKeys;
        private readonly object _typeId;
        #endregion

        
       
        public TraceActionFilter(object typeId,bool logPostData,string[] excludeFormDataKeys)
        {
            _typeId = typeId;
            _logPostData = logPostData;
            _excludeFormDataKeys = excludeFormDataKeys;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!ShouldExecute(context.ActionDescriptor)) return;
            LogActionExecution(context);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        public void OnResultExecuting(ResultExecutingContext context)
        {

        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            if (!ShouldExecute(context.ActionDescriptor)) return;
            LogResultExecuted(context);
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext executingContext, ActionExecutionDelegate next)
        {
            if (!ShouldExecute(executingContext.ActionDescriptor)) return;
            LogActionExecution(executingContext);
            await next();
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext executingContext, ResultExecutionDelegate next)
        {
            if (!ShouldExecute(executingContext.ActionDescriptor)) return;
            var executedContext = await next();
            LogResultExecuted(executedContext);

        }

        private void LogActionExecution(ActionExecutingContext context)
        {

            var actionExecutionLogger = InitiateLogger(context.HttpContext.RequestServices,
                ActionExecutionCategoryType);
            var actionArguments = context.ActionArguments;
            using (BeginScope(actionExecutionLogger, context.ActionDescriptor, "On Action Executing", context.HttpContext, actionArguments: actionArguments))
            {

                actionExecutionLogger.LogInformation("Action is Executing");

            }
        }

        private void LogResultExecuted(ResultExecutedContext context)
        {
            var resultLogger = InitiateLogger(context.HttpContext.RequestServices, ResultExecutionCategoryType);
            using (BeginScope(resultLogger, context.ActionDescriptor, "On Result Executing",
                context.HttpContext, actionArguments: null))
            {
                resultLogger.LogInformation("Result is Executed");
            }
        }

        private ScopeContext AppendBaseInfoToScope(ActionDescriptor actionDescriptor, HttpContext httpContext, IDictionary<string, object> actionArguments)
        {
            var scopeContext = new ScopeContext();
            var controllerActionDescriptor = actionDescriptor as ControllerActionDescriptor;
            scopeContext.Push("Controller", controllerActionDescriptor.ControllerName);
            scopeContext.Push("Action", controllerActionDescriptor.ActionName);
            scopeContext.Push("User", httpContext.User.Identity?.Name ?? "Not logged in");
            scopeContext.Push("Host", httpContext.Request.Host.Host);
            scopeContext.Push("Path", httpContext.Request.Path);
            scopeContext.Push("QueryString", LogHelper.Format(httpContext.Request?.Query, _excludeFormDataKeys));
            scopeContext.Push("HttpMethod", httpContext.Request.Method);
            if (_logPostData)
                scopeContext.Push("Parameters", LogHelper.Format(actionArguments, _excludeFormDataKeys));

            if (httpContext.Request.ContentType != null)
            {
                try
                {
                    if (_logPostData)
                        if (httpContext.Request.HasFormContentType)
                        {
                            scopeContext.Push("FormSubmit", httpContext.Request.ContentType == null ? null : LogHelper.Format(httpContext.Request.Form, _excludeFormDataKeys));
                        }
                }
                catch
                {
                    // ignored
                }
            }


            return scopeContext;

        }

        private IDisposable BeginScope(ILogger logger, ActionDescriptor actionDescriptor, string filterName, HttpContext httpContext, IDictionary<string, object> actionArguments)
        {
            var scopeContext = AppendBaseInfoToScope(actionDescriptor, httpContext, actionArguments);
            scopeContext.Push("Filter", filterName);
            var scope = scopeContext.BeginScope(logger);
            return scope;

        }

        private ILogger InitiateLogger(IServiceProvider serviceProvider, string categoryName)
        {

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            return loggerFactory.CreateLogger(string.Format(LoggerCategoryTypeName, categoryName));

        }

        private bool ShouldExecute(ActionDescriptor action)
        {
            var filterDescriptor = action.FilterDescriptors.OrderBy(fd => fd.Scope).LastOrDefault(fd => fd.Filter is TraceActionAttribute);
            if (filterDescriptor == null)
            {
                return true;
            }
            var filter = filterDescriptor.Filter as TraceActionAttribute;

            return filter.TypeId.Equals(_typeId);
        }

    }
}
