using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.AspNetCore.Logging
{
    public static class ExceptionHandlerExtensions
    {
        /// <summary>
        /// Adds a middleware to the pipeline that will catch pivotal internal exceptions, log them , set the <see cref="InternalExceptionHandlerFeature"/>
        /// The request will not be re-executed if the response has already started.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseInternalExceptionHandler(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            
            return app.UseMiddleware<InternalExceptionHandlerMiddleware>();
        }


        /// <summary>
        /// Adds a middleware to the pipeline that will catch pivotal internal exceptions, log them , set the <see cref="InternalExceptionHandlerFeature"/>
        /// The request will not be re-executed if the response has already started.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="errorHandlingPath"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseInternalExceptionHandler(this IApplicationBuilder app, string errorHandlingPath)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseInternalExceptionHandler(new InternalExceptionHandlerOptions()
            {
                ExceptionHandlingPath = new PathString(errorHandlingPath)
            });
        }

       
        /// <summary>
        /// Adds a middleware to the pipeline that will catch pivotal internal exceptions, log them , set the <see cref="InternalExceptionHandlerFeature"/>
        /// The request will not be re-executed if the response has already started.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseInternalExceptionHandler(this IApplicationBuilder app, InternalExceptionHandlerOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<InternalExceptionHandlerMiddleware>(Options.Create(options));
        }

        /// <summary>
        /// Adds a middleware to the pipeline that will catch pivotal internal exceptions, log them , set the <see cref="InternalExceptionHandlerFeature"/>
        /// The request will not be re-executed if the response has already started.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseInternalExceptionHandler(
            this IApplicationBuilder app,
            Action<IApplicationBuilder> configure)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            IApplicationBuilder applicationBuilder = app.New();
            configure(applicationBuilder);
            RequestDelegate requestDelegate = applicationBuilder.Build();
            return app.UseInternalExceptionHandler(new InternalExceptionHandlerOptions()
            {
                ExceptionHandler = requestDelegate
            });
        }

    }
}
