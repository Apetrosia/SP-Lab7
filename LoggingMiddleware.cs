using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GreenSwampApp
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("[{Time}] : {Method} {Path}",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                context.Request.Method,
                context.Request.Path);

            await _next(context);
        }
    }
}