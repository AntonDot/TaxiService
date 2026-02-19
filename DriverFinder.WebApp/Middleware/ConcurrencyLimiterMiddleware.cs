using System.Net;
using DriverFinder.WebApp.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace DriverFinder.WebApp.Middleware;

public class ConcurrencyLimiterMiddleware
{
    private readonly RequestDelegate next;
    private readonly SemaphoreSlim semaphore;
    private readonly ILogger<ConcurrencyLimiterMiddleware> logger;

    public ConcurrencyLimiterMiddleware(RequestDelegate next, IOptions<ParallelLimitSettings> options, ILogger<ConcurrencyLimiterMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
        var parallelLimit = options.Value.ParallelLimit;
        semaphore = new SemaphoreSlim(parallelLimit, parallelLimit);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!await semaphore.WaitAsync(0))
        {
            logger.LogWarning("Запрос от {IP} отклонен: превышен лимит одновременных запросов", context.Connection.RemoteIpAddress);
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            await context.Response.WriteAsync("Service is unavailable. Too many requests. Please try again later.");
            return;
        }

        try
        {
            await next(context);
        }
        finally
        {
            semaphore.Release();
        }
    }
}
