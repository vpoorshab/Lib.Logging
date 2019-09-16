using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Lib.Logging.Abstractions;



namespace Lib.AspNetCore.Logging
{
    public class TraceExceptionFilter : IExceptionFilter, IAsyncExceptionFilter
    {


        #region Fields & Properties

        private const string LoggerCategoryTypeName = "Lib.AspNetCore.Logging.TraceExceptionFilter";

        public bool LogPostData;
        public string[] ExcludeFormDataKeys;
        public object TypeId;
        public bool HandleException;
        #endregion

        #region Constructor

        public TraceExceptionFilter()
        {
            TypeId = new object();
            LogPostData = true;
            ExcludeFormDataKeys = Array.Empty<string>();
            HandleException = false;
        }

        #endregion

        #region Filter Methods


        public Task OnExceptionAsync(ExceptionContext exceptionContext)
        {

            var exception = InternalHelper.ConvertToInternalException(exceptionContext.HttpContext.TraceIdentifier, exceptionContext.Exception);
            LogException(exceptionContext.ActionDescriptor, exceptionContext.HttpContext, exception);
            exceptionContext.ExceptionHandled = HandleException;
            return Task.CompletedTask;

        }




        public void OnException(ExceptionContext exceptionContext)
        {
            var exception = InternalHelper.ConvertToInternalException(exceptionContext.HttpContext.TraceIdentifier, exceptionContext.Exception);
            LogException(exceptionContext.ActionDescriptor, exceptionContext.HttpContext, exception);
            exceptionContext.ExceptionHandled = HandleException;

        }

        #endregion

        #region Private Methods

        private ScopeContext AppendBaseInfoToScope(ActionDescriptor actionDescriptor, HttpContext httpContext, IDictionary<string, object> actionArguments)
        {
            var scopeContext = new ScopeContext();
            var controllerActionDescriptor = actionDescriptor as ControllerActionDescriptor;
            scopeContext.Push("Controller", controllerActionDescriptor.ControllerName);
            scopeContext.Push("Action", controllerActionDescriptor.ActionName);
            scopeContext.Push("User", httpContext.User.Identity?.Name ?? "Not logged in");
            scopeContext.Push("Host", httpContext.Request.Host.Host);
            scopeContext.Push("Path", httpContext.Request.Path);
            scopeContext.Push("QueryString", LogHelper.Format(httpContext.Request?.Query, ExcludeFormDataKeys));
            scopeContext.Push("HttpMethod", httpContext.Request.Method);
            if (LogPostData)
                scopeContext.Push("Parameters", LogHelper.Format(actionArguments, ExcludeFormDataKeys));

            if (httpContext.Request.ContentType != null)
            {
                try
                {
                    if (LogPostData)
                        if (httpContext.Request.HasFormContentType)
                        {
                            scopeContext.Push("FormSubmit", httpContext.Request.ContentType == null ? null : LogHelper.Format(httpContext.Request.Form, ExcludeFormDataKeys));
                        }
                }
                catch
                {
                    // ignored
                }
            }


            return scopeContext;

        }

        private void LogException(ActionDescriptor actionDescriptor, HttpContext httpContext, InternalException exception)
        {
            if (!ShouldExecute(actionDescriptor)) return;


            if (exception.ExceptionHandled) return;

            var resultLogger = InitiateLogger(httpContext.RequestServices);
            var scopeContext = new ScopeContext();
            scopeContext.Push(AppendBaseInfoToScope(actionDescriptor, httpContext, null));
            scopeContext.Push(InternalHelper.CreateExceptionScopeContext(actionDescriptor, httpContext, exception));
            exception.ExceptionHandled = true;

            using (resultLogger.BeginScope(scopeContext.ToDictionary()))
            {

                resultLogger.LogError(exception, exception.Message);

            }

        }


        #endregion

        private ILogger InitiateLogger(IServiceProvider serviceProvider)
        {

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            return loggerFactory.CreateLogger(LoggerCategoryTypeName);

        }

        private bool ShouldExecute(ActionDescriptor action)
        {
            var filterDescriptor = action.FilterDescriptors.OrderBy(fd => fd.Scope).LastOrDefault(fd => fd.Filter is TraceExceptionFilter);
            if (filterDescriptor == null)
            {
                return true;
            }
            var filter = filterDescriptor.Filter as TraceExceptionFilter;
            return filter.TypeId.Equals(TypeId);
        }
    }

    
}
