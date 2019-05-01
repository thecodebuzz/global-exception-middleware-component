using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SampleService
{
    public static class ExceptionMiddleware
    {
        public static void UseApiExceptionHandler(this IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if(contextFeature!=null)
                    {
                        //log the error
                        var logger = loggerFactory.CreateLogger("GlobalException");
                        logger.LogError($"Something went wrong: { contextFeature.Error}");

                        //report the error to client 
                        await context.Response.WriteAsync(new GlobalErrorDetails()
                        {
                            StatusCodeContext = context.Response.StatusCode,
                            Message ="Something Went Wrong.Please try again later"


                        }.ToString());
                    }

                });

             });

        }
    }

    public static class GloablExceptionMiddleware
    {
        public static void UseGloablExceptionMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }

    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _logger = new LoggerFactory().AddConsole().CreateLogger("GlobalException");//Lets create new console logger overriding default logging
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                // Call the next delegate/ middleware in the pipeline
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                await HandleGlobalExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleGlobalExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

            return context.Response.WriteAsync(new GlobalErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = "Something went wrong.Internal Server Error! "
            }.ToString());
        }
    }
}
