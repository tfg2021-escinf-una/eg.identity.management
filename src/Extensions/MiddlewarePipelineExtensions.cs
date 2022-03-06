using EG.IdentityManagement.Microservice.Customizations.Middleware;
using Microsoft.AspNetCore.Builder;
using System.Diagnostics.CodeAnalysis;

namespace EG.IdentityManagement.Microservice.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class MiddlewarePipelineExtensions
    {
        public static IApplicationBuilder UseMiddlewarePipeline(
            this IApplicationBuilder builder)
        {
            // Insert here all the middlewares you want to add.

            return builder.UseMiddleware<GlobalHandlerException>();
        }
    }
}