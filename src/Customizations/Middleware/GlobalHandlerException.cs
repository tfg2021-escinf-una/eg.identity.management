using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;

namespace EG.IdentityManagement.Microservice.Customizations.Middleware
{
    [ExcludeFromCodeCoverage]
    public class GlobalHandlerException
    {
        private readonly ILogger<GlobalHandlerException> _logger;
        private readonly RequestDelegate _next;

        public GlobalHandlerException(RequestDelegate next,
                                      ILogger<GlobalHandlerException> logger)
        {
            _next = next ?? throw new ArgumentNullException("Request Delegate should not come as null");
            _logger = logger ?? throw new ArgumentNullException("Request Delegate should not come as null");
        }

        public async Task InvokeAsync(HttpContext currentContext)
        {
            try
            {
                await _next(currentContext);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An unhandled critical error has occurred, please take a look the logs.");
                await HandleExceptionAsync(currentContext,
                                           ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            await context.Response.WriteAsync(JsonConvert.SerializeObject(new
            {
                ErrorType = "Internal Server Error",
                Description = "An unhandled critical error has occurred in the microservice. Please contact support",
                Actions = "Check logs!"
            }));
        }
    }
}