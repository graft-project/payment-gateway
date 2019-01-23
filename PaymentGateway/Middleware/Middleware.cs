using Graft.Infrastructure;
using Graft.Infrastructure.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace PaymentGateway.Middleware
{
    public class Middleware
    {
        readonly RequestDelegate _next;

        public Middleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ApiException ex)
            {
                // we must return OK status code to the terminal
                //context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                string json = JsonConvert.SerializeObject(new ApiErrorResult(ex.Error));
                await context.Response.WriteAsync(json);
            }
            catch (Exception ex)
            {
                if (IsApiCall(context.Request))
                {
                    if (context.Response.HasStarted)
                        throw;

                    //context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";

                    string exceptionMessage = ex.Message;
                    if (ex.InnerException != null && ex.InnerException.Message != ex.Message)
                        exceptionMessage += $" ({ex.InnerException.Message})";

                    string json = JsonConvert.SerializeObject(new ApiErrorResult(new ApiError(ErrorCode.InternalServerError, exceptionMessage)));

                    await context.Response.WriteAsync(json);
                }
                else
                {
                    throw;
                }
            }
        }

        public bool IsApiCall(HttpRequest request)
        {
            //bool isJson = request.GetTypedHeaders().Accept.Contains(
            //    new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
            //if (isJson)
            //    return true;
            if (request.Path.Value.StartsWith("/v1.0/"))
                return true;
            return false;
        }
    }

    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Middleware>();
        }
    }
}
