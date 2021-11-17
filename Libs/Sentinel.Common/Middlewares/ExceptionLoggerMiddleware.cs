using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sentinel.Common.Middlewares
{
    public class ExceptionLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ExceptionLoggerOptions _options;
        private readonly ILogger _logger;

        public ExceptionLoggerMiddleware(
            RequestDelegate next,
            IOptions<ExceptionLoggerOptions> options,
            ILoggerFactory loggerFactory
            )
        {

            _options = options.Value;
            _logger = loggerFactory.CreateLogger<ExceptionLoggerMiddleware>();
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                var unhandledException = LoggerMessage.Define(LogLevel.Error, new EventId(1, "UnhandledException"), "An unhandled exception has occurred while executing the request.");
                unhandledException(_logger, ex);

                if (httpContext.Response.HasStarted)
                {
                    var response = "Response has started";
                    if (_options != null)
                    {
                        response = _options.Name + " " + response;
                    }
                    _logger.LogDebug(response);
                }
                throw;
            }
        }
    }
    public class ExceptionLoggerOptions
    {
        public string Name { get; set; } = default!;
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ExceptionLoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionLogger(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionLoggerMiddleware>();
        }
    }
}